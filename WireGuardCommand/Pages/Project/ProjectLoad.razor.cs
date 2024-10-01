using Microsoft.AspNetCore.Components;

using WireGuardCommand.Components.Models;
using WireGuardCommand.Services;
using WireGuardCommand.Services.Models;

namespace WireGuardCommand.Pages.Project;

public partial class ProjectLoad
{
    [Inject]
    public AlertController AlertController { get; set; } = default!;

    [Inject]
    public NavigationManager NavigationManager { get; set; } = default!;

    [Inject]
    public ProjectManager ProjectManager { get; set; } = default!;

    [Inject]
    public ProjectCache Cache { get; set; } = default!;

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if(Cache.CurrentProject.Metadata is null)
        {
            AlertController.Push(AlertType.Error, "Failed to load project metadata.");
            return;
        }

        if(Cache.CurrentProject.Metadata.IsEncrypted)
        {
            return;
        }

        try
        {
            var data = await ProjectManager.LoadProjectAsync(Cache.CurrentProject.Metadata);
            Cache.CurrentProject.ProjectData = data;

            NavigationManager.NavigateTo("ProjectView");
        }
        catch(Exception ex)
        {
            AlertController.Push(AlertType.Error, $"Failed to load project: {ex.Message}");
            StateHasChanged();
        }
    }

    private async Task DecryptProjectAsync()
    {
        if (Cache.CurrentProject.Metadata is null)
        {
            AlertController.Push(AlertType.Error, "Failed to load project metadata");
            return;
        }

        if(string.IsNullOrWhiteSpace(Cache.CurrentProject.Passphrase))
        {
            AlertController.Push(AlertType.Error, "You need to enter a passphrase to decrypt the project.");
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
            AlertController.Push(AlertType.Error, $"Failed to decrypt project: {ex.Message}");
        }
    }

    private void GoBack()
    {
        Cache.Clear();
        NavigationManager.NavigateTo("/");
    }
}
