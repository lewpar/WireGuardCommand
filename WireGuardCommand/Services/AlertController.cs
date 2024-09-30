using WireGuardCommand.Components;
using WireGuardCommand.Events;

namespace WireGuardCommand.Services;

public class AlertController
{
    public event EventHandler<AlertPushedEventArgs>? AlertPushed;

    public void Push(AlertType type, string message, int lifetime = 0)
    {
        AlertPushed?.Invoke(this, new AlertPushedEventArgs(type, message, lifetime));
    }
}
