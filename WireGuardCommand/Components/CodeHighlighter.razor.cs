using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

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
        await JSRuntime.InvokeVoidAsync("hljs.highlightAll");
    }
}
