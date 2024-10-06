using Microsoft.AspNetCore.Components;

namespace WireGuardCommand.Components;

public partial class TabMenu
{
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    public List<TabPage> Pages { get; set; } = new List<TabPage>();
    public TabPage? CurrentPage { get; set; }

    public void AddPage(TabPage page)
    {
        Pages.Add(page);

        // Make the first page added the default page.
        if(page.Default)
        {
            CurrentPage = page;
        }

        StateHasChanged();
    }

    public async Task SetPageAsync(TabPage page)
    {
        CurrentPage = page;

        if(page.OnPageLoad.HasDelegate)
        {
            await page.OnPageLoad.InvokeAsync(this);
        }

        StateHasChanged();
    }
}
