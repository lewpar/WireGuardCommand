using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Components;

using System.Diagnostics;
using System.Net;
using System.Reflection;
using System.Runtime.Versioning;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using ICSharpCode.SharpZipLib.Zip;
using WireGuardCommand.Components;
using WireGuardCommand.Components.Models;
using WireGuardCommand.Configuration;
using WireGuardCommand.Extensions;
using WireGuardCommand.IO;
using WireGuardCommand.Services;
using WireGuardCommand.Services.Models;
using WireGuardCommand.WireGuard;

namespace WireGuardCommand.Pages.Project;

public partial class ProjectView
{
    [Inject]
    public AlertController AlertController { get; set; } = default!;

    [Inject]
    public ProjectManager ProjectManager { get; set; } = default!;

    [Inject]
    public ProjectCache Cache { get; set; } = default!;

    [Inject]
    public NavigationManager NavigationManager { get; set; } = default!;

    [Inject]
    public ILogger<ProjectView> Logger { get; set; } = default!;

    [Inject]
    public WGCConfig Config { get; set; } = default!;

    private record PreviewConfig(string Title, string Code, int? PeerId = null);
    private List<PreviewConfig> PreviewConfigs { get; set; } = new List<PreviewConfig>();
    private string PreviewCode { get; set; } = "";
    private bool loadingPreview;

    private bool HasUnsavedChanges => HasChanges();

    private ProjectData? originalData;

    private Dialog? dialog;

    protected override void OnInitialized()
    {
        if(Cache.CurrentProject.ProjectData is null)
        {
            return;
        }

        originalData = Cache.CurrentProject.ProjectData.Clone();
        Logger.LogInformation("Loaded project.");
    }

    private void CloseProject()
    {
        if(HasUnsavedChanges)
        {
            if(dialog is null)
            {
                return;
            }

            dialog.Show(DialogType.YesNo, "Unsaved Changes", "You have unsaved changes, are you sure you want to close the project?", async () =>
            {
                await Task.Run(() =>
                {
                    NavigationManager.NavigateTo("/");
                    Cache.Clear();
                });
            });
        }
        else
        {
            NavigationManager.NavigateTo("/");
            Cache.Clear();
        }
    }

    private void RegenerateSeed()
    {
        dialog?.Show(DialogType.YesNo, "Regenerate Seed", "Are you sure you want to regenerate the project seed?<br/>This is <b>irreversible</b> and will require you to redeploy all of your peers.", async () =>
        {
            await Task.Run(() =>
            {
                var project = Cache.CurrentProject;
                if (project.ProjectData is null)
                {
                    AlertController.Push(AlertType.Error, "Failed to generate seed.");
                    return;
                }

                project.ProjectData.Seed = RandomNumberGenerator.GetBytes(Config.SeedSize / 8).ToBase64();
            });

            await InvokeAsync(StateHasChanged);
        });
    }

    private bool HasChanges()
    {
        if (originalData is null ||
            Cache.CurrentProject.ProjectData is null)
        {
            AlertController.Push(AlertType.Error, "Unable to determine changes.");
            return false;
        }

        return JsonSerializer.Serialize(originalData) != JsonSerializer.Serialize(Cache.CurrentProject.ProjectData);
    }

    public async Task SaveChangesAsync()
    {
        if(Cache.CurrentProject.ProjectData is null)
        {
            return;
        }

        var project = Cache.CurrentProject.ProjectData;

        try
        {
            await ProjectManager.SaveProjectAsync(Cache.CurrentProject);

            AlertController.Push(AlertType.Info, "Saved changes.", 4000);
        }
        catch (Exception ex)
        {
            AlertController.Push(AlertType.Error, $"Failed to save project: {ex.Message}");
            StateHasChanged();
            return;
        }

        originalData = project.Clone();
        StateHasChanged();
    }

    private async Task GenerateConfigsAsync()
    {
        if (HasValidationErrors())
        {
            AlertController.Push(AlertType.Error, "Failed to generate configs: This project has validation errors.");
            return;
        }
        
        if (Cache.CurrentProject.ProjectData is null ||
            Cache.CurrentProject.Metadata is null)
        {
            return;
        }

        var metadata = Cache.CurrentProject.Metadata;

        if(string.IsNullOrWhiteSpace(metadata.Path))
        {
            AlertController.Push(AlertType.Error, "Failed to generate: Failed to find path to project.");
            return;
        }

        var data = Cache.CurrentProject.ProjectData;
        if (data is null)
        {
            AlertController.Push(AlertType.Error, "Failed to get project data.");
            return;
        }

        try
        {
            var outputPath = Path.Combine(metadata.Path, "Output");
            if(!Directory.Exists(outputPath))
            {
                Directory.CreateDirectory(outputPath);
            }

            var writer = new WireGuardWriter();
            var server = GenerateServerPeer();
            
            await using (var fsServer = File.OpenWrite(Path.Combine(outputPath, "server.conf")))
            {
                await writer.WriteAsync(server, fsServer);
            }

            foreach (var peer in server.Peers)
            {
                await using (var fsClient = File.OpenWrite(Path.Combine(outputPath, $"peer-{peer.Id}.conf")))
                {
                    await writer.WriteAsync(peer, fsClient);   
                }
            }

            string? commands = GenerateCustomCommands();
            if(!string.IsNullOrWhiteSpace(commands))
            {
                if (string.IsNullOrWhiteSpace(data.CommandFileName))
                {
                    AlertController.Push(AlertType.Error, "You must set a file name for the evaluator output.");
                    return;
                }

                var filePath = Path.Combine(outputPath, data.CommandFileName);

                if (string.IsNullOrWhiteSpace(data.CommandOnce) &&
                    string.IsNullOrWhiteSpace(data.CommandPerPeer))
                {
                    if (File.Exists(filePath))
                    {
                        File.Delete(filePath);
                    }
                }

                await File.WriteAllTextAsync(filePath, commands);
            }

            if (Cache.CurrentProject.ProjectData.IsZippedOutput)
            {
                await ZipOutputAsync(outputPath, Cache.CurrentProject.ProjectData.ZipPassphrase);
            }

            AlertController.Push(AlertType.Info, "Generated configuration.", 4000);
        }
        catch(Exception ex)
        {
            AlertController.Push(AlertType.Error, $"Failed to generate configs: {ex.Message}");
        }
    }

    public string? GenerateCustomCommands()
    {
        var project = Cache.CurrentProject;

        var data = project.ProjectData;
        if(data is null)
        {
            return null;
        }

        try
        {
            var server = GenerateServerPeer();
            var sb = new StringBuilder();

            if (!string.IsNullOrWhiteSpace(data.CommandOnce))
            {
                sb.AppendLine(ReplaceVariables(server, data.CommandOnce));
            }

            if(!string.IsNullOrWhiteSpace(data.CommandPerPeer))
            {
                foreach(var peer in server.Peers)
                {
                    sb.AppendLine(ReplaceVariables(server, data.CommandPerPeer, peer.Id));
                }
            }

            return sb.ToString();
        }
        catch(Exception ex)
        {
            AlertController.Push(AlertType.Error, $"Failed to generate custom commands: {ex.Message}");
            return null;
        }
    }

    private string ReplaceVariables(WireGuardPeer server, string content, int? peerId = null)
    {
        var project = Cache.CurrentProject;

        var data = project.ProjectData;
        if (data is null)
        {
            return content;
        }

        // Generic vars
        content = content.Replace("{interface.name}", data.Interface);
        content = content.Replace("{allowed.ip}", data.AllowedIPs);

        // Server vars
        content = content.Replace("{server.endpoint}", data.Endpoint);
        content = content.Replace("{server.address}", server.Address.ToString());
        content = content.Replace("{server.port}", server.ListenPort.ToString());

        content = content.Replace("{server.privatekey}", server.PrivateKey.ToString());
        content = content.Replace("{server.publickey}", server.PublicKey.ToString());

        // Peer vars
        if (peerId is not null)
        {
            var peer = server.Peers.FirstOrDefault(p => p.Id == peerId);
            if(peer is null)
            {
                return content;
            }

            content = content.Replace("{peer.id}", peerId.ToString());

            content = content.Replace("{peer.address}", peer.Address.ToString());
            content = content.Replace("{peer.port}", peer.ListenPort.ToString());

            content = content.Replace("{peer.privatekey}", peer.PrivateKey.ToString());
            content = content.Replace("{peer.publickey}", peer.PublicKey.ToString());

            if (peer.PresharedKey is not null)
            {
                content = content.Replace("{peer.presharedkey}", peer.PresharedKey.ToString());
            }
        }

        return content;
    }

    [SupportedOSPlatform("Windows")]
    private void BrowseProject()
    {
        var metadata = Cache.CurrentProject.Metadata;

        if (metadata is null ||
            string.IsNullOrWhiteSpace(metadata.Path))
        {
            AlertController.Push(AlertType.Error, "Failed to open project path, no path was found.");
            return;
        }

        Process.Start("explorer.exe", metadata.Path);
    }

    private WireGuardPeer GenerateServerPeer()
    {
        var project = Cache.CurrentProject.ProjectData;
        if(project is null)
        {
            throw new Exception("Failed to load project data.");
        }

        var subnet = project.Subnet.Split('/');
        if (subnet.Length < 2)
        {
            throw new Exception("Invalid transit subnet, ensure you have included a CIDR.");
        }

        if (!byte.TryParse(subnet[1], out byte cidr))
        {
            throw new Exception("Invalid transit subnet CIDR.");
        }

        var ipSubnet = new IPNetwork2(IPAddress.Parse(subnet[0]), cidr);
        var builder = new WireGuardBuilder(new WireGuardBuilderOptions()
        {
            Seed = project.Seed.FromBase64(),
            Subnet = ipSubnet,
            ListenPort = project.ListenPort,
            AllowedIPs = project.AllowedIPs,
            Endpoint = project.Endpoint,
            DNS = project.DNS,
            UseLastAddress = project.UseLastAddress,
            UsePresharedKeys = project.UsePresharedKeys,
            PostUp = project.PostUp,
            PostDown = project.PostDown
        });

        if(ipSubnet.Usable < project.NumberOfClients)
        {
            throw new Exception("The number of clients exceeds the max usable hosts for the transit subnet.");
        }

        for (int i = 0; i < project.NumberOfClients; i++)
        {
            builder.AddPeer();
        }

        return builder.Build();
    }

    private async Task GeneratePreviewAsync()
    {
        loadingPreview = true;

        PreviewConfigs.Clear();
        PreviewCode = "";
        
        if (HasValidationErrors())
        {
            AlertController.Push(AlertType.Error, "Failed to create preview: This project has validation errors.");
            loadingPreview = false;
            return;
        }

        try
        {
            var server = GenerateServerPeer();

            var writer = new WireGuardWriter();

            using (var ms = new MemoryStream())
            {
                await writer.WriteAsync(server, ms);

                var config = Encoding.UTF8.GetString(ms.ToArray());
                PreviewConfigs.Add(new PreviewConfig("Server", config));
            }

            foreach (var peer in server.Peers)
            {
                using (var ms = new MemoryStream())
                {
                    await writer.WriteAsync(peer, ms);

                    var config = Encoding.UTF8.GetString(ms.ToArray());
                    PreviewConfigs.Add(new PreviewConfig($"Peer {peer.Id}", config, peer.Id));
                }
            }

            PreviewCode = GenerateCustomCommands() ?? "";
        }
        catch(Exception ex)
        {
            AlertController.Push(AlertType.Error, $"Failed to generate preview: {ex.Message}");
        }

        await Task.Delay(500);
        loadingPreview = false;
    }

    private async Task SaveAsTemplateAsync()
    {
        if (HasValidationErrors())
        {
            AlertController.Push(AlertType.Error, "Failed to create template: This project has validation errors.");
            return;
        }
        
        var template = Cache.CurrentProject.CreateTemplate();
        if(template is null)
        {
            AlertController.Push(AlertType.Error, "Failed to save template.");
            return;
        }

        await ProjectManager.SaveTemplateAsync(template);

        AlertController.Push(AlertType.Info, "Saved Template.", 4000);
    }

    private void ShowVariables()
    {
        dialog?.Show(DialogType.Ok, "Variables",
            """
            These variables are replaced when the configuration is generated.
            <br/><br/>
            <b>Generic</b>
            <ul>
            <li>{interface.name} - The name of the interface.</li>
            <li>{allowed.ip} - The allowed IPs that can connect.</li>
            </ul>
            <b>Server</b>
            <ul>
            <li>{server.endpoint} - The server endpoint.</li>
            <li>{server.address} - The address for the server.</li>
            <li>{server.port} - The port the server is listening on.</li>
            <li>{server.privatekey} - The servers private key.</li>
            <li>{server.publickey} - The servers public key.</li>
            </ul>
            <b>Peer</b>
            <ul>
            <li>{peer.id} - The peer id.</li>
            <li>{peer.address} - The address for the peer.</li>
            <li>{peer.port} - The port the peer is listening on.</li>
            <li>{peer.privatekey} - The peers private key.</li>
            <li>{peer.publickey} - The peers public key.</li>
            <li>{peer.presharedkey} - The preshared key between the server and peer.</li>
            </ul>
            """);
    }

    private bool HasValidationErrors()
    {
        var project = Cache.CurrentProject;
        var data = project.ProjectData;

        if (data is null)
        {
            return true;
        }

        try
        {
            Validator.ValidateObject(data,
                new ValidationContext(data, null, null),
                true);

            return false;
        }
        catch
        {
            return true;
        }
    }


    private async Task ZipOutputAsync(string path, string? passphrase = null)
    {
        using var fs = File.OpenWrite(Path.Combine(path, "output.zip"));
        using var zipStream = new ZipOutputStream(fs);
        
        if (!string.IsNullOrWhiteSpace(passphrase))
        {
            zipStream.Password = passphrase;
        }
        
        var files = Directory.GetFiles(path, "*", SearchOption.AllDirectories)
            .Where(f => !f.EndsWith(".zip"));
        
        foreach(var file in files)
        {
            var fileName = Path.GetFileName(file);
            var newEntry = new ZipEntry(fileName)
            {
                DateTime = DateTime.Now
            };
            
            zipStream.PutNextEntry(newEntry);
            
            using (var configStream = File.OpenRead(file))
            {
                await configStream.CopyToAsync(zipStream);
                zipStream.CloseEntry();
            }
            
            File.Delete(file);
        }
    }

    private void ShowAbout()
    {
        dialog?.Show(DialogType.Ok, "WireGuard Command", $"v{GetVersion()}<br/><br/>Built on Blazor (ASP.NET + SignalR) and Electron.NET<br/><br/>Powered by <a class='link' href='https://github.com/lewpar/WireGuardCommand'>open-source</a>");
    }

    private string? GetVersion()
    {
        var assembly = Assembly.GetExecutingAssembly();
        var fileVersion = FileVersionInfo.GetVersionInfo(assembly.Location);
        
        return fileVersion.ProductVersion;
    }
}
