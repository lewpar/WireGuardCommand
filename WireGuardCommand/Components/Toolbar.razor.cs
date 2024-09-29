using Microsoft.AspNetCore.Components;

namespace WireGuardCommand.Components;

public partial class Toolbar
{
    [Parameter]
    public RenderFragment? ChildContent { get; set; }
}
