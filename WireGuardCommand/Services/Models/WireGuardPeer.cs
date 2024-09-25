using System.Net;

using WireGuardCommand.Security;

namespace WireGuardCommand.Services.Models;

public class WireGuardPeer
{
    public int Id { get; set; }

    public IPNetwork2 Subnet { get; set; }
    public IPAddress Address { get; set; }
    public int Port { get; set; }

    public string AllowedIPs { get; set; }

    public CurveKey PublicKey { get; set; }
    public CurveKey PrivateKey { get; set; }

    public string DNS { get; set; }
    public string Endpoint { get; set; }

    public WireGuardPeer()
    {
        Id = 0;

        Subnet = new IPNetwork2();
        Address = new IPAddress(0);
        Port = 51820;

        AllowedIPs = "0.0.0.0/0, ::/0";

        PublicKey = new CurveKey();
        PrivateKey = new CurveKey();

        DNS = "";
        Endpoint = "";
    }
}
