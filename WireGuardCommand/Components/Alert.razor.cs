using Microsoft.AspNetCore.Components;

using WireGuardCommand.Components.Models;
using WireGuardCommand.Events;
using WireGuardCommand.Services;

namespace WireGuardCommand.Components;

/// <summary>
/// Creates an alert component that displays an alert message at the top of the page.
/// </summary>
public partial class Alert
{
    [Inject]
    public AlertController AlertController { get; set; } = default!;

    [Parameter]
    public AlertPosition Position { get; set; } = AlertPosition.Bottom;

    private AlertType type = AlertType.Info;
    private string? content;

    private int lifetime = 0;
    private string animationStyle = "";
    private string lifetimeAnimationStyle = "";

    protected override void OnInitialized()
    {
        AlertController.AlertPushed += AlertController_AlertPushed;
    }

    private async void AlertController_AlertPushed(object? sender, AlertPushedEventArgs e)
    {
        await InvokeAsync(async() =>
        {
            Dismiss();
            StateHasChanged();

            animationStyle = $"animation-name: {(Position == AlertPosition.Top ? "dropdown" : "dropup")}";

            type = e.Type;
            content = e.Message;
            lifetime = e.Lifetime;

            StateHasChanged();

            if (lifetime != 0)
            {
                lifetimeAnimationStyle = $"animation-name: expand; animation-duration: {lifetime / 1000}s";
                StateHasChanged();

                await Task.Delay(lifetime);

                animationStyle = $"animation-name: {(Position == AlertPosition.Top ? "dropup" : "dropdown")}";
                StateHasChanged();

                // Delay before actually removing the alert, must match the animation duration in Alert.razor.css.
                await Task.Delay(500);

                content = "";
                StateHasChanged();
            }
        });
    }

    private void Dismiss()
    {
        content = string.Empty;
    }
}
