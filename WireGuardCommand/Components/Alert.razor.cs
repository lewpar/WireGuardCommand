using Microsoft.AspNetCore.Components;

namespace WireGuardCommand.Components;

/// <summary>
/// Creates an alert component that displays a alert message at the top of the page.
/// </summary>
public partial class Alert
{
    [Parameter]
    public AlertType Type { get; set; } = AlertType.Info;

    [Parameter]
    public string? Content { get; set; }

    [Parameter]
    public EventCallback OnDismiss { get; set; }

    private void Dismiss()
    {
        this.Content = string.Empty;

        if(OnDismiss.HasDelegate)
        {
            OnDismiss.InvokeAsync(null);
        }
    }
}
