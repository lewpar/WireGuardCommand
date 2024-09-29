using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using QRCoder;
using WireGuardCommand.Extensions;
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

    private string qrCode = "";
    public Dialog QRCodeDialog { get; set; } = default!;

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if(JSRuntime is null)
        {
            return;
        }

        qrCode = $"<img src='data:image/png;base64, {GetQRCode(Code).ToBase64()}' width='256' height='256' alt='qr code'/>";

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

    private byte[] GetQRCode(string content)
    {
        using (QRCodeGenerator qrGenerator = new QRCodeGenerator())
        using (QRCodeData qrCodeData = qrGenerator.CreateQrCode(content, QRCodeGenerator.ECCLevel.Q))
        using (PngByteQRCode qrCode = new PngByteQRCode(qrCodeData))
        {
            return qrCode.GetGraphic(20);
        }
    }
}
