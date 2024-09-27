using System.Security.Cryptography;

using WireGuardCommand.Extensions;

namespace WireGuardCommand.Services.Models;

public class ProjectData
{
    public string Interface { get; set; }

    public string Seed { get; set; }
    public int NumberOfClients { get; set; }
    public string Subnet { get; set; }
    public bool UsePresharedKeys { get; set; }
    public string DNS { get; set; }
    public string Endpoint { get; set; }
    public int ListenPort { get; set; }
    public string AllowedIPs { get; set; }
    public bool UseLastAddress { get; set; }

    public bool IsZippedOutput { get; set; }
    public string ZipPassphrase { get; set; }

    public ProjectData()
    {
        Interface = "wg0";

        Seed = RandomNumberGenerator.GetBytes(256).ToBase64();
        NumberOfClients = 3;
        Subnet = "10.0.0.0/24";
        UsePresharedKeys = false;
        DNS = "";
        Endpoint = "remote.endpoint.net:51820";
        ListenPort = 51820;
        AllowedIPs = "0.0.0.0/0, ::/0";
        UseLastAddress = false;

        IsZippedOutput = false;
        ZipPassphrase = "";
    }

    public ProjectData Clone()
    {
        return new ProjectData()
        {
            Interface = Interface,

            Seed = Seed,
            NumberOfClients = NumberOfClients,
            Subnet = Subnet,
            UsePresharedKeys = UsePresharedKeys,
            DNS = DNS,
            Endpoint = Endpoint,
            ListenPort = ListenPort,
            AllowedIPs = AllowedIPs,
            UseLastAddress = UseLastAddress,

            IsZippedOutput = IsZippedOutput,
            ZipPassphrase = ZipPassphrase
        };
    }
}
