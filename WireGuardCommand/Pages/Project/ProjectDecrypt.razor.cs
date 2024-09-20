using Microsoft.AspNetCore.Components;

using WireGuardCommand.Services;

namespace WireGuardCommand.Pages.Project;

public partial class ProjectDecrypt
{
    [Inject]
    public NavigationManager NavigationManager { get; set; } = default!;

    [Inject]
    public ProjectManager ProjectManager { get; set; } = default!;

    public string? Error { get; set; }

    private async Task DecryptProjectAsync()
    {
        var result = await ProjectManager.TryDecryptDataAsync();
        if(!result.Success)
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
