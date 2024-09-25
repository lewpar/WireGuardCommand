namespace WireGuardCommand.Security;

public class CurveKey
{
    public byte[]? Key { get; set; }

    public CurveKey()
    {
        Key = null;
    }

    public override string ToString()
    {
        return (Key != null) ? Convert.ToBase64String(Key) : string.Empty;
    }
}
