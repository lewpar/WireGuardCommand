using Microsoft.AspNetCore.Components;

namespace WireGuardCommand.Components;

public partial class ToolbarMenuItem
{
    [Parameter]
    public string Title { get; set; } = "";

    [Parameter]
    public EventCallback OnClick { get; set; }

    private async Task OnClickMenuItemAsync()
    {
        if(!OnClick.HasDelegate)
        {
            return;
        }

        await OnClick.InvokeAsync(null);
    }
}
