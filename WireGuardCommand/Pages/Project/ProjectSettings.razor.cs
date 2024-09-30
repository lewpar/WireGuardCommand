using Microsoft.AspNetCore.Components;

namespace WireGuardCommand.Pages.Project;

public partial class ProjectSettings
{
    [Inject]
    public NavigationManager NavigationManager { get; set; } = default!;

    private void GoBack()
    {
        NavigationManager.NavigateTo("/");
    }
}
