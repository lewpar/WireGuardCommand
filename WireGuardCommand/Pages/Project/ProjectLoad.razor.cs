using Microsoft.AspNetCore.Components;

using WireGuardCommand.Services;
using WireGuardCommand.Services.Models;

namespace WireGuardCommand.Pages.Project;

public partial class ProjectLoad
{
    [Inject]
    public NavigationManager NavigationManager { get; set; } = default!;

    [Inject]
    public ProjectManager ProjectManager { get; set; } = default!;

    [Inject]
    public ProjectCache Cache { get; set; } = default!;

    public string? Error { get; set; }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if(Cache.CurrentProject.Metadata is null)
        {
            Error = "Failed to load project metadata.";
            return;
        }

        if(Cache.CurrentProject.Metadata.IsEncrypted)
        {
            return;
        }

        try
        {
            ProjectData data = await ProjectManager.LoadProjectAsync(Cache.CurrentProject.Metadata);
            Cache.CurrentProject.ProjectData = data;

            NavigationManager.NavigateTo("ProjectView");
        }
        catch(Exception ex)
        {
            Error = $"Failed to load project: {ex.Message}";
            return;
        }
    }

    private async Task DecryptProjectAsync()
    {
        if (Cache.CurrentProject.Metadata is null)
        {
            Error = "Failed to load project metadata.";
            return;
        }

        if(string.IsNullOrWhiteSpace(Cache.CurrentProject.Passphrase))
        {
            Error = "You need to enter a passphrase to decrypt the project.";
            return;
        }

        try
        {
            ProjectData data = await ProjectManager.LoadProjectAsync(Cache.CurrentProject.Metadata, 
                                                             Cache.CurrentProject.Passphrase);
            Cache.CurrentProject.ProjectData = data;

            NavigationManager.NavigateTo("ProjectView");
        }
        catch(Exception ex)
        {
            Error = $"Failed to decrypt project: {ex.Message}";
            return;
        }
    }

    private void GoBack()
    {
        Cache.Clear();
        NavigationManager.NavigateTo("/");
    }
}
