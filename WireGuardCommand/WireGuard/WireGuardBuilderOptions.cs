using System.Net;

namespace WireGuardCommand.WireGuard;

public class WireGuardBuilderOptions
{
    public required byte[] Seed { get; init; }
    public required IPNetwork2 Subnet { get; init; }

    public required int ListenPort { get; init; }
    public required string AllowedIPs { get; init; }

    public string? Endpoint { get; init; }
    public string? DNS { get; init; }
    
    public string? PostUp { get; init; }
    public string? PostDown { get; init; }

    public bool UseLastAddress { get; init; }
    public bool UsePresharedKeys { get; init; }
}
