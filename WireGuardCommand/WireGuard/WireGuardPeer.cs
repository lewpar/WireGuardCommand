using System.Net;

using WireGuardCommand.Security;

namespace WireGuardCommand.WireGuard;

public class WireGuardPeer
{
    public int Id { get; init; }

    public WireGuardPeerRole Role { get; init; } = WireGuardPeerRole.Client;

    public IPNetwork2 Subnet { get; init; } = new();
    public IPAddress Address { get; init; } = new(0);
    public int ListenPort { get; init; } = 51820;

    public string AllowedIPs { get; init; } = "0.0.0.0/0, ::/0";

    public CurveKey PublicKey { get; init; } = new();
    public CurveKey PrivateKey { get; init; } = new();
    public CurveKey? PresharedKey { get; init; }

    public string? DNS { get; init; }
    public string? Endpoint { get; init; }

    public string? PostUp { get; init; }
    public string? PostDown { get; init; }

    public List<WireGuardPeer> Peers { get; set; } = new();
}
