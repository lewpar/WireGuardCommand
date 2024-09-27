using Microsoft.AspNetCore.Components;

namespace WireGuardCommand.Components;

public partial class Dialog
{
    [Parameter]
    public string Title { get; set; } = "";

    [Parameter]
    public string Content { get; set; } = "";

    [Parameter]
    public DialogType Type { get; set; } = DialogType.Ok;

    [Parameter]
    public EventCallback ClickOk { get; set; }

    [Parameter]
    public EventCallback ClickYes { get; set; }

    [Parameter]
    public EventCallback ClickNo { get; set; }

    private bool isVisible;

    private void OnClickOk()
    {
        Hide();

        if (ClickOk.HasDelegate)
        {
            ClickOk.InvokeAsync(null);
        }
    }

    private void OnClickYes()
    {
        Hide();

        if (ClickYes.HasDelegate)
        {
            ClickYes.InvokeAsync(null);
        }
    }

    private void OnClickNo()
    {
        Hide();

        if (ClickNo.HasDelegate)
        {
            ClickNo.InvokeAsync(null);
        }
    }

    public void Show()
    {
        isVisible = true;
        StateHasChanged();
    }

    public void Hide()
    {
        isVisible = false;
        StateHasChanged();
    }
}
