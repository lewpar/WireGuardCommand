using Microsoft.AspNetCore.Components;

namespace WireGuardCommand.Components;

public partial class Alert
{
    [Parameter]
    public string Type { get; set; } = "Status";

    [Parameter]
    public string? Content { get; set; }

    [Parameter]
    public EventCallback<string> ContentChanged { get; set; }
}
