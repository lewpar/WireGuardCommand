using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using static System.Net.Mime.MediaTypeNames;

namespace WireGuardCommand.Components;

public partial class CodeHighlighter
{
    [Inject]
    public IJSRuntime JSRuntime { get; set; } = default!;

    [Parameter]
    public string Language { get; set; } = "language-ini";

    [Parameter]
    public string Code { get; set; } = "";

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if(JSRuntime is null)
        {
            return;
        }

        await JSRuntime.InvokeVoidAsync("hljs.highlightAll");
    }

    private async Task CopyToClipboardAsync()
    {
        if(JSRuntime is null)
        {
            return;
        }

        await JSRuntime.InvokeVoidAsync("navigator.clipboard.writeText", Code);
    }
}
