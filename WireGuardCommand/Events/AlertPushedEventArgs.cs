using WireGuardCommand.Components;

namespace WireGuardCommand.Events;

public class AlertPushedEventArgs : EventArgs
{
    public AlertType Type { get; set; }
    public string Message { get; }

    public AlertPushedEventArgs(AlertType type, string message)
    {
        Type = type;
        Message = message;
    }
}
