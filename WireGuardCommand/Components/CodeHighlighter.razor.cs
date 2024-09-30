using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

using QRCoder;

using WireGuardCommand.Extensions;
using WireGuardCommand.Services;

namespace WireGuardCommand.Components;

public partial class CodeHighlighter
{
    [Inject]
    public AlertController AlertController { get; set; } = default!;

    [Inject]
    public IJSRuntime JSRuntime { get; set; } = default!;

    [Parameter]
    public string Language { get; set; } = "language-ini";

    [Parameter]
    public string Code { get; set; } = "";

    [Parameter]
    public string FileName { get; set; } = "wg.conf";

    private string qrCode = "";
    public Dialog QRCodeDialog { get; set; } = default!;

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if(JSRuntime is null)
        {
            return;
        }

        try
        {
            var qrCodeData = GetQRCode(Code);
            qrCode = $"<img src='data:image/png;base64, {qrCodeData.ToBase64()}' width='256' height='256' alt='qr code'/>";
        }
        catch(Exception ex)
        {
            qrCode = $"Failed to generate QR Code: {ex.Message}";
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
        AlertController.Push(AlertType.Info, "Copied to clipboard.", 4000);
    }

    private byte[] GetQRCode(string content)
    {
        using (QRCodeGenerator qrGenerator = new QRCodeGenerator())
        using (QRCodeData qrCodeData = qrGenerator.CreateQrCode(content, QRCodeGenerator.ECCLevel.M))
        using (PngByteQRCode qrCode = new PngByteQRCode(qrCodeData))
        {
            return qrCode.GetGraphic(20);
        }
    }

    private async Task SaveConfigAsync()
    {
        if(JSRuntime is null)
        {
            return;
        }

        await JSRuntime.InvokeVoidAsync("SaveTextToFile", new[] { FileName,  Code });
    }

    private void ShowQRCode()
    {
        QRCodeDialog.Show(DialogType.Ok, "QR Code", qrCode);
    }
}
