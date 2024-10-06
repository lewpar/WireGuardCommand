using Microsoft.AspNetCore.Components;

using WireGuardCommand.Components;
using WireGuardCommand.Configuration;
using WireGuardCommand.Services.Models;
using WireGuardCommand.Services;

using System.Runtime.Versioning;
using System.Diagnostics;
using WireGuardCommand.Components.Models;

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
    public WGCConfig Config { get; set; } = default!;

    private List<ProjectMetadata> projects = new List<ProjectMetadata>();

    private ProjectMetadata? selectedProject;

    private bool loaded;

    private Dialog? dialog;

    protected override async Task OnInitializedAsync()
    {
        await LoadProjectsAsync();
    }

    private async Task LoadProjectsAsync()
    {
        loaded = false;

        await Task.Delay(500);

        try
        {
            projects = await ProjectManager.GetProjectsAsync();
        }
        catch (Exception ex)
        {
            AlertController.Push(AlertType.Error, $"Failed to load projects: {ex.Message}");
        }

        loaded = true;
    }

    private void SelectProject(ProjectMetadata project)
    {
        selectedProject = project;
    }

    private void OpenProject()
    {
        Cache.CurrentProject.Metadata = selectedProject;
        NavigationManager.NavigateTo("ProjectLoad");
    }

    private void CreateProject()
    {
        NavigationManager.NavigateTo("ProjectCreate");
    }

    [SupportedOSPlatform("Windows")]
    private void BrowseProjects()
    {
        Process.Start("explorer.exe", Config.ProjectsPath);
    }

    private void PromptDeleteProject()
    {
        if(dialog is null)
        {
            return;
        }

        dialog.Show(DialogType.YesNo, "Delete Project", $"Are you sure you want to delete <b>{selectedProject?.Name}</b>?", async () =>
        {
            DeleteProject();
            await LoadProjectsAsync();
            StateHasChanged();
        });
    }

    private void DeleteProject()
    {
        if(selectedProject is null)
        {
            return;
        }

        try
        {
            ProjectManager.DeleteProject(selectedProject);
            Cache.Clear();
            selectedProject = null;
        }
        catch(Exception ex)
        {
            AlertController.Push(AlertType.Error, $"Failed to delete project: {ex.Message}");
        }
    }

    private void OpenSettings()
    {
        NavigationManager.NavigateTo("ProjectSettings");
    }
}
