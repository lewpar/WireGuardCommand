using System.Net;

using WireGuardCommand.Security;

namespace WireGuardCommand.WireGuard;

public class WireGuardPeer
{
    public int Id { get; set; }

    public WireGuardPeerRole Role { get; set; }

    public IPNetwork2 Subnet { get; set; }
    public IPAddress Address { get; set; }
    public int ListenPort { get; set; }

    public string AllowedIPs { get; set; }

    public CurveKey PublicKey { get; set; }
    public CurveKey PrivateKey { get; set; }
    public CurveKey? PresharedKey { get; set; }

    public string? DNS { get; set; }
    public string? Endpoint { get; set; }

    public List<WireGuardPeer> Peers { get; set; }

    public WireGuardPeer()
    {
        Id = 0;

        Role = WireGuardPeerRole.Client;

        Subnet = new IPNetwork2();
        Address = new IPAddress(0);
        ListenPort = 51820;

        AllowedIPs = "0.0.0.0/0, ::/0";

        PublicKey = new CurveKey();
        PrivateKey = new CurveKey();
        PresharedKey = null;

        DNS = "";
        Endpoint = "";

        Peers = new List<WireGuardPeer>();
    }
}
