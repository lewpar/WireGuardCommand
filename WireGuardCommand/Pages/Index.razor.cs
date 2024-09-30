using Microsoft.AspNetCore.Components;

using WireGuardCommand.Configuration;
using WireGuardCommand.Services.Models;
using WireGuardCommand.Services;

using System.Runtime.Versioning;
using System.Diagnostics;

using Microsoft.Extensions.Options;
using WireGuardCommand.Components;

namespace WireGuardCommand.Pages;

public partial class Index
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
    public IOptions<WGCConfig> Options { get; set; } = default!;

    public List<ProjectMetadata> Projects { get; set; } = new List<ProjectMetadata>();

    public ProjectMetadata? SelectedProject { get; set; }

    public bool Loaded { get; set; }

    public Dialog? Dialog { get; set; }

    protected override async Task OnInitializedAsync()
    {
        await LoadProjectsAsync();
    }

    private async Task LoadProjectsAsync()
    {
        Loaded = false;

        try
        {
            Projects = await ProjectManager.GetProjectsAsync();
        }
        catch (Exception ex)
        {
            AlertController.Push(AlertType.Error, $"Failed to load projects: {ex.Message}");
        }

        Loaded = true;
    }

    private void SelectProject(ProjectMetadata project)
    {
        SelectedProject = project;
    }

    private void OpenProject()
    {
        Cache.CurrentProject.Metadata = SelectedProject;
        NavigationManager.NavigateTo("ProjectLoad");
    }

    private void CreateProject()
    {
        NavigationManager.NavigateTo("ProjectCreate");
    }

    [SupportedOSPlatform("Windows")]
    private void BrowseProjects()
    {
        var config = Options.Value;

        Process.Start("explorer.exe", config.ProjectsPath);
    }

    private void PromptDeleteProject()
    {
        if(Dialog is null)
        {
            return;
        }

        Dialog.Show(DialogType.YesNo, "Delete Project", $"Are you sure you want to delete <b>{SelectedProject?.Name}</b>?", async () =>
        {
            DeleteProject();
            await LoadProjectsAsync();
            StateHasChanged();
        });
    }

    private void DeleteProject()
    {
        if(SelectedProject is null)
        {
            return;
        }

        try
        {
            ProjectManager.DeleteProject(SelectedProject);
            Cache.Clear();
            SelectedProject = null;
        }
        catch(Exception ex)
        {
            AlertController.Push(AlertType.Error, $"Failed to delete project: {ex.Message}");
        }
    }
}
