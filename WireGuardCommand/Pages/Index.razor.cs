using Microsoft.AspNetCore.Components;

using WireGuardCommand.Services.Models;
using WireGuardCommand.Services;

namespace WireGuardCommand.Pages;

public partial class Index
{
    [Inject]
    public ProjectManager ProjectManager { get; set; } = default!;

    [Inject]
    public NavigationManager NavigationManager { get; set; } = default!;

    public ProjectMetadata? SelectedProject { get; set; }

    protected override async Task OnParametersSetAsync()
    {
        await ProjectManager.LoadProjectsAsync();
    }

    private void SelectProject(ProjectMetadata project)
    {
        SelectedProject = project;
    }

    private void OpenProject(ProjectMetadata project)
    {
        ProjectManager.CurrentProject = project;

        NavigationManager.NavigateTo(project.IsEncrypted ? "ProjectDecrypt" : "ProjectView");
    }

    private void DeleteProject(ProjectMetadata project)
    {
        ProjectManager.CurrentProject = project;
        NavigationManager.NavigateTo("ProjectDelete");
    }

    private void CreateProject()
    {
        NavigationManager.NavigateTo("ProjectCreate");
    }
}
