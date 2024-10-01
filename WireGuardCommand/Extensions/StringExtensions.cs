namespace WireGuardCommand.Extensions;

public static class StringExtensions
{
    public static byte[] FromBase64(this string base64)
    {
        return Convert.FromBase64String(base64);
    }
}
