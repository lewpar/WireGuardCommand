using Microsoft.AspNetCore.Components;

using WireGuardCommand.Services;

namespace WireGuardCommand.Pages.Project;

public partial class ProjectLoad
{
    [Inject]
    public NavigationManager NavigationManager { get; set; } = default!;

    [Inject]
    public ProjectManager ProjectManager { get; set; } = default!;

    public string? Error { get; set; }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if(ProjectManager.CurrentProject is null)
        {
            return;
        }

        if(ProjectManager.CurrentProject.Metadata is null)
        {
            return;
        }

        if(ProjectManager.CurrentProject.Metadata.IsEncrypted)
        {
            return;
        }

        var result = await ProjectManager.TryLoadProjectAsync();
        if(!result.Success)
        {
            Error = result.Message;
            return;
        }

        NavigationManager.NavigateTo("ProjectView");
    }

    private async Task DecryptProjectAsync()
    {
        var result = await ProjectManager.TryLoadProjectAsync();
        if (!result.Success)
        {
            Error = result.Message;
            return;
        }
    }

    private void GoBack()
    {
        NavigationManager.NavigateTo("/");
    }
}
