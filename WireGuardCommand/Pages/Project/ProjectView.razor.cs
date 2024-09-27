using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Options;

using System.Diagnostics;
using System.Runtime.Versioning;
using System.Security.Cryptography;
using System.Text.Json;
using WireGuardCommand.Components;
using WireGuardCommand.Configuration;
using WireGuardCommand.Extensions;
using WireGuardCommand.IO;
using WireGuardCommand.Services;
using WireGuardCommand.Services.Models;

namespace WireGuardCommand.Pages.Project;

public partial class ProjectView
{
    [Inject]
    public ProjectManager ProjectManager { get; set; } = default!;

    [Inject]
    public ProjectCache Cache { get; set; } = default!;

    [Inject]
    public NavigationManager NavigationManager { get; set; } = default!;

    [Inject]
    public ILogger<ProjectView> Logger { get; set; } = default!;

    [Inject]
    public IOptions<WGCConfig> Options { get; set; } = default!;

    public enum ProjectViewTab
    {
        General,
        Export
    }

    public ProjectViewTab CurrentTab { get; set; } = ProjectViewTab.General;

    public string? Status { get; set; }
    public string? Error { get; set; }

    public bool HasUnsavedChanges
    {
        get => HasChanges();
    }

    private ProjectData? originalData;

    public Dialog? Dialog { get; set; }
    public string DialogTitle { get; set; } = "";
    public string DialogContent { get; set; } = "";
    public Action DialogYes { get; set; } = () => { };

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
            if(Dialog is null)
            {
                return;
            }

            DialogTitle = "Unsaved Changes";
            DialogContent = "You have unsaved changes, are you sure you want to close the project?";
            Dialog.Show();

            DialogYes = () =>
            {
                NavigationManager.NavigateTo("/");
                Cache.Clear();
            };
        }
        else
        {
            NavigationManager.NavigateTo("/");
            Cache.Clear();
        }
    }

    private void RegenerateSeed()
    {
        Error = "";

        if (Dialog is null)
        {
            return;
        }

        DialogTitle = "Regenerate Seed";
        DialogContent = "Are you sure you want to regenerate the project seed?<br/>This is <b>irreversable</b> and will require you to redeploy all of your peers.";
        Dialog.Show();

        DialogYes = () =>
        {
            var project = Cache.CurrentProject;
            if (project.ProjectData is null)
            {
                Error = "Failed to generate seed.";
                return;
            }

            var config = Options.Value;

            project.ProjectData.Seed = RandomNumberGenerator.GetBytes(config.SeedSize / 8).ToBase64();
        };
    }

    private bool HasChanges()
    {
        Error = "";

        if(originalData is null || 
            Cache.CurrentProject.ProjectData is null)
        {
            Error = "Unable to determine changes.";
            return false;
        }

        return JsonSerializer.Serialize(originalData) != JsonSerializer.Serialize(Cache.CurrentProject.ProjectData);
    }

    public async Task SaveChangesAsync()
    {
        Error = "";
        Status = "";

        if(Cache.CurrentProject.ProjectData is null)
        {
            return;
        }

        var project = Cache.CurrentProject.ProjectData;

        try
        {
            await ProjectManager.SaveProjectAsync(Cache.CurrentProject);
            Status = "Saved changes.";
        }
        catch (Exception ex)
        {
            Error = $"Failed to save project: {ex.Message}";
            StateHasChanged();
            return;
        }

        originalData = project.Clone();
        StateHasChanged();
    }

    public async Task GenerateConfigsAsync()
    {
        Error = "";
        Status = "";

        if (Cache.CurrentProject.ProjectData is null ||
            Cache.CurrentProject.Metadata is null)
        {
            return;
        }

        var project = Cache.CurrentProject.ProjectData;
        var metadata = Cache.CurrentProject.Metadata;

        if(string.IsNullOrWhiteSpace(metadata.Path))
        {
            Error = "Failed to generate: Failed to find path to project.";
            return;
        }

        try
        {
            var outputPath = Path.Combine(metadata.Path, "Output");
            if(!Directory.Exists(outputPath))
            {
                Directory.CreateDirectory(outputPath);
            }

            var writer = new ProjectWriter(project);
            await writer.WriteConfigsAsync(outputPath);

            Status = "Generated configuration.";
        }
        catch(Exception ex)
        {
            Error = $"Failed to generate configs: {ex.Message}";
        }
    }

    [SupportedOSPlatform("Windows")]
    private void BrowseProject()
    {
        Error = "";

        var metadata = Cache.CurrentProject.Metadata;

        if (metadata is null ||
            string.IsNullOrWhiteSpace(metadata.Path))
        {
            Error = "Failed to open project path, no path was found.";
            return;
        }

        Process.Start("explorer.exe", metadata.Path);
    }
}
