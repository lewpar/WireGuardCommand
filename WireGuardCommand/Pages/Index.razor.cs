using Microsoft.AspNetCore.Components;

using WireGuardCommand.Configuration;
using WireGuardCommand.Services.Models;
using WireGuardCommand.Services;

using System.Runtime.Versioning;
using System.Diagnostics;

using Microsoft.Extensions.Options;

namespace WireGuardCommand.Pages;

public partial class Index
{
    [Inject]
    public ProjectManager ProjectManager { get; set; } = default!;

    [Inject]
    public NavigationManager NavigationManager { get; set; } = default!;

    [Inject]
    public IOptions<WGCConfig> Options { get; set; } = default!;

    public ProjectMetadata? SelectedProject { get; set; }

    protected override async Task OnInitializedAsync()
    {
        await ProjectManager.LoadProjectsAsync();
    }

    private void SelectProject(ProjectMetadata project)
    {
        SelectedProject = project;
    }

    private void OpenProject(ProjectMetadata project)
    {
        ProjectManager.CurrentProject.Metadata = project;

        NavigationManager.NavigateTo("ProjectLoad");
    }

    private void DeleteProject(ProjectMetadata project)
    {
        ProjectManager.CurrentProject.Metadata = project;
        NavigationManager.NavigateTo("ProjectDelete");
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
}
