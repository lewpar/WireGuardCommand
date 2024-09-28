using System.Net;

namespace WireGuardCommand.WireGuard;

public class WireGuardBuilderOptions
{
    public required byte[] Seed { get; set; }
    public required IPNetwork2 Subnet { get; set; }

    public required int ListenPort { get; set; }
    public required string AllowedIPs { get; set; }

    public bool UseLastAddress { get; set; }
    public bool UsePresharedKeys { get; set; }
}
