using WireGuardCommand.Components.Models;

namespace WireGuardCommand.Events;

public class AlertPushedEventArgs : EventArgs
{
    public AlertType Type { get; set; }

    public string Message { get; }
    public int Lifetime { get; set; }

    public AlertPushedEventArgs(AlertType type, string message, int lifetime = 0)
    {
        Type = type;
        Message = message;
        Lifetime = lifetime;
    }
}
