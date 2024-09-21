using Microsoft.Extensions.Options;

using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

using WireGuardCommand.Configuration;
using WireGuardCommand.Extensions;
using WireGuardCommand.Models;
using WireGuardCommand.Security;
using WireGuardCommand.Services.Models;

namespace WireGuardCommand.Services;

public class ProjectManager
{
    public List<ProjectMetadata> Projects { get; private set; }

    public ProjectContext CurrentProject { get; set; }

    private readonly IServiceProvider serviceProvider;

    public ProjectManager(IServiceProvider serviceProvider)
    {
        CurrentProject = new ProjectContext();

        Projects = new List<ProjectMetadata>();
        this.serviceProvider = serviceProvider;
    }

    public async Task<ActionResult> TryLoadProjectAsync()
    {
        if(CurrentProject is null)
        {
            return ActionResult.Fail("No project currently loaded.");
        }

        if(string.IsNullOrWhiteSpace(CurrentProject.Path))
        {
            return ActionResult.Fail("No path was found for current project.");
        }

        try
        {
            if(CurrentProject.Metadata is null)
            {
                return ActionResult.Fail("Failed to load metadata for current project.");
            }

            var dataPath = Path.Combine(CurrentProject.Path, 
                CurrentProject.Metadata.IsEncrypted ? "data.bin" : "data.json");

            using (var fs = File.OpenRead(dataPath))
            {
                if(CurrentProject.Metadata.IsEncrypted)
                {
                    if(string.IsNullOrWhiteSpace(CurrentProject.Passphrase))
                    {
                        return ActionResult.Fail("No passphrase found for current project.");
                    }

                    if (string.IsNullOrEmpty(CurrentProject.Metadata.Salt))
                    {
                        return ActionResult.Fail("Failed to retrieve Salt from metadata.");
                    }

                    if (string.IsNullOrEmpty(CurrentProject.Metadata.IV))
                    {
                        return ActionResult.Fail("Failed to retrieve IV from metadata.");
                    }

                    var salt = CurrentProject.Metadata.Salt.FromBase64();
                    var iv = CurrentProject.Metadata.IV.FromBase64();

                    var data = AESProvider.Decrypt(fs, AESProvider.GenerateKey(CurrentProject.Passphrase, salt), iv);
                    if(data is null)
                    {
                        return ActionResult.Fail("Failed to decrypt project.");
                    }

                    using var ms = new MemoryStream(data);
                    CurrentProject.ProjectData = await JsonSerializer.DeserializeAsync<ProjectData>(ms);
                }
                else
                {
                    CurrentProject.ProjectData = await JsonSerializer.DeserializeAsync<ProjectData>(fs);
                }
            }

            return ActionResult.Pass();
        }
        catch(Exception ex)
        {
            return ActionResult.Fail($"An error occured while trying to load the project: {ex.Message}");
        }
    }

    public async Task LoadProjectsAsync()
    {
        Projects.Clear();

        using var scope = serviceProvider.CreateAsyncScope();

        var options = scope.ServiceProvider.GetService<IOptions<WGCConfig>>();
        if(options is null)
        {
            throw new Exception("Failed to get configuration service.");
        }

        var config = options.Value;

        if(!Directory.Exists(config.ProjectsPath))
        {
            Directory.CreateDirectory(config.ProjectsPath);
        }

        var projects = Directory.GetDirectories(config.ProjectsPath);
        foreach(var project in projects)
        {
            var metadataPath = Path.Combine(project, "project.json");
            if(!File.Exists(metadataPath))
            {
                continue;
            }

            var result = await TryLoadMetadataAsync(metadataPath);
            if(!result.Success)
            {
                throw new Exception(result.Message);
            }

            if(result.Result is null)
            {
                throw new NullReferenceException("Metadata result in null.");
            }

            var metadata = result.Result;

            if(Projects.Contains(metadata))
            {
                continue;
            }

            Projects.Add(metadata);
        }
    }

    private async Task<ActionResult<ProjectMetadata>> TryLoadMetadataAsync(string metadataPath)
    {
        using var fs = File.OpenRead(metadataPath);

        var metadata = await JsonSerializer.DeserializeAsync<ProjectMetadata>(fs);
        if(metadata is null)
        {
            return new ActionResult<ProjectMetadata>(false, $"Failed to deserialize '{metadataPath}'.");
        }

        var path = Path.GetFullPath(metadataPath);

        metadata.Path = path;
        CurrentProject.Path = Path.GetDirectoryName(path);

        return new ActionResult<ProjectMetadata>(true, result: metadata);
    }

    public async Task<ActionResult<ProjectData>> TryDecryptDataAsync()
    {
        var metadata = CurrentProject.Metadata;
        if(metadata is null)
        {
            return new ActionResult<ProjectData>(false, "No project is currently loaded.");
        }

        if(string.IsNullOrWhiteSpace(CurrentProject.Path))
        {
            return new ActionResult<ProjectData>(false, "No path was set for current project.");
        }

        if (CurrentProject.Passphrase is null || 
            CurrentProject.Passphrase.Length == 0)
        {
            return new ActionResult<ProjectData>(false, "No passphrase was set.");
        }

        var path = Path.Combine(CurrentProject.Path, "project.bin");
        if(!File.Exists(path))
        {
            return new ActionResult<ProjectData>(false, "No project data found.");
        }

        var data = await File.ReadAllBytesAsync(CurrentProject.Path);

        return new ActionResult<ProjectData>(true);
    }

    public async Task<ActionResult> TryCreateProjectAsync(ProjectCreateContext createContext)
    {
        try
        {
            if(string.IsNullOrWhiteSpace(createContext.Path))
            {
                return ActionResult.Fail("No project path is set.");
            }

            if(string.IsNullOrWhiteSpace(createContext.Name))
            {
                return ActionResult.Fail("No project name is set.");
            }

            if(createContext.IsEncrypted &&
                string.IsNullOrWhiteSpace(createContext.Passphrase))
            {
                return ActionResult.Fail("No passphrase is set.");
            }

            if (Directory.Exists(createContext.Path) &&
                Directory.GetFiles(createContext.Path).Length != 0)
            {
                return ActionResult.Fail("A project already exists at this path.");
            }

            Directory.CreateDirectory(createContext.Path);

            var metadataPath = Path.Combine(createContext.Path, "project.json");
            var metadata = new ProjectMetadata()
            {
                Name = createContext.Name,
                IsEncrypted = createContext.IsEncrypted,
                Path = metadataPath,
                Salt = RandomNumberGenerator.GetBytes(32).ToBase64(),
                IV = AESProvider.GenerateIV().ToBase64()
            };

            using (var fs = File.OpenWrite(metadataPath))
            {
                await JsonSerializer.SerializeAsync<ProjectMetadata>(fs, metadata);
            }

            var dataPath = Path.Combine(createContext.Path, metadata.IsEncrypted ? "data.bin" : "data.json");
            var data = new ProjectData()
            {
                Seed = RandomNumberGenerator.GetBytes(32).ToBase64()
            };

            using (var fs = File.OpenWrite(dataPath))
            {
                if(metadata.IsEncrypted)
                {
                    var key = AESProvider.GenerateKey(createContext.Passphrase, metadata.Salt.FromBase64());
                    using var ms = new MemoryStream();

                    await JsonSerializer.SerializeAsync<ProjectData>(ms, data);
                    var encryptedData = AESProvider.Encrypt(ms.ToArray(), key, metadata.IV.FromBase64());

                    fs.Write(encryptedData, 0, encryptedData.Length);
                }
                else
                {
                    await JsonSerializer.SerializeAsync<ProjectData>(fs, data);
                }
            }
        }
        catch(Exception ex)
        {
            return ActionResult.Fail($"{ex.Message}");
        }

        return ActionResult.Pass();
    }
}
