using Microsoft.AspNetCore.Components;

using WireGuardCommand.Events;
using WireGuardCommand.Services;

namespace WireGuardCommand.Components;

/// <summary>
/// Creates an alert component that displays a alert message at the top of the page.
/// </summary>
public partial class Alert
{
    [Inject]
    public AlertController AlertController { get; set; } = default!;

    public AlertType Type { get; set; } = AlertType.Info;
    public string? Content { get; set; }

    protected override void OnInitialized()
    {
        AlertController.AlertPushed += AlertController_AlertPushed;
    }

    private void AlertController_AlertPushed(object? sender, AlertPushedEventArgs e)
    {
        Dismiss();

        InvokeAsync(() =>
        {
            StateHasChanged();
        });

        Type = e.Type;
        Content = e.Message;

        InvokeAsync(() =>
        {
            StateHasChanged();
        });
    }

    private void Dismiss()
    {
        this.Content = string.Empty;
    }
}
