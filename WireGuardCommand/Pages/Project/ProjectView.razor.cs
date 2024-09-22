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
    public ProjectCache Cache { get; set; } = default!;

    [Inject]
    public NavigationManager NavigationManager { get; set; } = default!;

    private void GoBack()
    {
        Cache.Clear();
        NavigationManager.NavigateTo("/");
    }

    private void GenerateSeed()
    {
        var project = Cache.CurrentProject;
        if(project.ProjectData is null)
        {
            return;
        }

        project.ProjectData.Seed = RandomNumberGenerator.GetBytes(32).ToBase64();
    }
}
