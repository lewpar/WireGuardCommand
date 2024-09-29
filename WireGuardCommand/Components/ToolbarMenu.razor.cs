using Microsoft.AspNetCore.Components;

namespace WireGuardCommand.Components;

public partial class ToolbarMenu
{
    [Parameter]
    public string Title { get; set; } = "";

    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    private bool menuVisible;

    private void ToggleMenu()
    {
        menuVisible = !menuVisible;
    }

    private void CloseMenu()
    {
        menuVisible = false;
    }
}
