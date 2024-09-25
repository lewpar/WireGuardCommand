using System.Net;

namespace WireGuardCommand.Extensions;

public static class StringExtensions
{
    public static byte[] FromBase64(this string base64)
    {
        return Convert.FromBase64String(base64);
    }

    public static bool TryParseAddress(this string address, out IPNetwork2 result)
    {
        try
        {
            result = IPNetwork2.Parse(address);
            return true;
        }
        catch
        {
            result = new IPNetwork2();
            return false;
        }
    }
}
