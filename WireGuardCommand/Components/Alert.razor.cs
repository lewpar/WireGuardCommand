using Microsoft.AspNetCore.Components;
using WireGuardCommand.Components.Models;

namespace WireGuardCommand.Components;

public partial class Alert
{
    [Parameter]
    public AlertType Type { get; set; } = AlertType.Info;

    [Parameter]
    public string? Content { get; set; }

    [Parameter]
    public EventCallback<string> ContentChanged { get; set; }
}
