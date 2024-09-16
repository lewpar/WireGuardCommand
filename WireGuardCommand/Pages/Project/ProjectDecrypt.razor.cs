using Microsoft.AspNetCore.Components;

using WireGuardCommand.Services;

namespace WireGuardCommand.Pages.Project;

public partial class ProjectDecrypt
{
    [Inject]
    public NavigationManager NavigationManager { get; set; } = default!;

    [Inject]
    public ProjectManager ProjectManager { get; set; } = default!;

    public string? Passphrase { get; set; }

    public string? Error { get; set; }

    private void DecryptProject()
    {
        if(string.IsNullOrWhiteSpace(Passphrase))
        {
            Error = "You must enter a passphrase to decrypt the project.";
            return;
        }
    }

    private void GoBack()
    {
        NavigationManager.NavigateTo("/");
    }
}
