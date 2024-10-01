using Microsoft.AspNetCore.Components;

using WireGuardCommand.Components;
using WireGuardCommand.Configuration;
using WireGuardCommand.Services;

namespace WireGuardCommand.Pages.Project;

public partial class ProjectSettings
{
    [Inject]
    public NavigationManager NavigationManager { get; set; } = default!;

    [Inject]
    public AlertController AlertController { get; set; } = default!;

    [Inject]
    public WGCConfig Config { get; set; } = default!;

    private void GoBack()
    {
        NavigationManager.NavigateTo("/");
    }

    private async Task SaveAsync()
    {
        try
        {
            await Config.SaveAsync();
            AlertController.Push(AlertType.Info, $"Saved configuration.", 4000);
        }
        catch(Exception ex)
        {
            AlertController.Push(AlertType.Error, $"Failed to save configuration: {ex.Message}");
        }
    }
}
