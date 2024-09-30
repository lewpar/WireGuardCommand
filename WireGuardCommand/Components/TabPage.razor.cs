using Microsoft.AspNetCore.Components;

namespace WireGuardCommand.Components;

public partial class TabPage
{
    [Parameter]
    public string Title { get; set; } = default!;

    [Parameter]
    public bool Default { get; set; }

    [CascadingParameter]
    public TabMenu TabMenu { get; set; } = default!;

    [Parameter]
    public RenderFragment ChildContent { get; set; } = default!;

    [Parameter]
    public EventCallback OnPageLoad { get; set; }

    protected override void OnInitialized()
    {
        TabMenu.AddPage(this);
    }
}
