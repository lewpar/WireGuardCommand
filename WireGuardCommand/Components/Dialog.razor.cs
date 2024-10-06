using Microsoft.AspNetCore.Components;
using WireGuardCommand.Components.Models;

namespace WireGuardCommand.Components;

public partial class Dialog
{
    public string Title { get; set; } = "";
    public string Content { get; set; } = "";
    public DialogType Type { get; set; } = DialogType.Ok;
    public Func<Task>? Action { get; set; }

    private bool isVisible;
    private RenderFragment? componentFragment;

    private async Task OnClickYesAsync()
    {
        Hide();

        if(Action is not null)
        {
            await Action.Invoke();
        }
    }

    private void OnClickNo()
    {
        Hide();
    }

    public void Show(DialogType type, string title, string content, Func<Task>? onClickYes = null, Type? component = null)
    {
        Type = type;
        Title = title;
        Content = content;
        Action = onClickYes;

        isVisible = true;
        
        StateHasChanged();
    }

    public void Show(string title, Type component)
    {
        Type = DialogType.Component;
        Title = title;
        
        componentFragment = builder =>
        {
            builder.OpenComponent(0, component);
            builder.CloseComponent();
        };
        
        isVisible = true;
        
        StateHasChanged();
    }

    public void Hide()
    {
        isVisible = false;
        StateHasChanged();
    }
}
