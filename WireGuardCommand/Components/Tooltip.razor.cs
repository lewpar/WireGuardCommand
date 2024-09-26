using Microsoft.AspNetCore.Components;

namespace WireGuardCommand.Components;

/// <summary>
/// Creates a tooltip component that displays information when moused over.
/// </summary>
public partial class Tooltip
{
    /// <summary>
    /// Sets the title of the tooltip. Does not support HTML.
    /// </summary>
    [Parameter]
    public string Title { get; set; } = "";

    /// <summary>
    /// Sets the content of the tooltip. Supports HTML.
    /// </summary>
    [Parameter]
    public string Content { get; set; } = "";
}
