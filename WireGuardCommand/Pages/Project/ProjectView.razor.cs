using Microsoft.AspNetCore.Components;
using System.Security.Cryptography;
using WireGuardCommand.Extensions;
using WireGuardCommand.Services;

namespace WireGuardCommand.Pages.Project;

public partial class ProjectView
{
    [Inject]
    public ProjectManager ProjectManager { get; set; } = default!;

    [Inject]
    public NavigationManager NavigationManager { get; set; } = default!;

    private void GoBack()
    {
        NavigationManager.NavigateTo("/");
    }

    private void GenerateSeed()
    {
        var project = ProjectManager.CurrentProject;
        if(project is null ||
            project.ProjectData is null)
        {
            return;
        }

        project.ProjectData.Seed = RandomNumberGenerator.GetBytes(32).ToBase64();
    }
}
