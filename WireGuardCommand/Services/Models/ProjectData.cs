using System.Security.Cryptography;

using WireGuardCommand.Extensions;

namespace WireGuardCommand.Services.Models;

public class ProjectData
{
    public string Seed { get; set; }
    public int NumberOfClients { get; set; }
    public string Subnet { get; set; }
    public bool UsePresharedKeys { get; set; }
    public string DNS { get; set; }
    public string Endpoint { get; set; }
    public int ListenPort { get; set; }
    public string AllowedIPs { get; set; }
    public bool UseLastAddress { get; set; }

    public ProjectData()
    {
        Seed = RandomNumberGenerator.GetBytes(32).ToBase64();
        NumberOfClients = 3;
        Subnet = "10.0.0.0/24";
        UsePresharedKeys = false;
        DNS = "";
        Endpoint = "remote.endpoint.net:51820";
        ListenPort = 51820;
        AllowedIPs = "0.0.0.0/0, ::/0";
        UseLastAddress = false;
    }

    public ProjectData Copy()
    {
        return new ProjectData()
        {
            Seed = Seed,
            NumberOfClients = NumberOfClients
        };
    }
}
