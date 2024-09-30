namespace WireGuardCommand.Services.Models;

public class ProjectTemplate
{
    public string Name { get; set; }

    public int ListenPort { get; set; }
    public int NumberOfClients { get; set; }

    public string DNS { get; set; }
    public string Endpoint { get; set; }

    public string Subnet { get; set; }
    public string AllowedIPs { get; set; }

    public bool UseLastAddress { get; set; }
    public bool UsePresharedKeys { get; set; }

    public string PostUp { get; set; }
    public string PostDown { get; set; }

    public string CommandOnce { get; set; }
    public string CommandPerPeer { get; set; }
    public string CommandFileName { get; set; }

    public ProjectTemplate()
    {
        Name = "Default";

        DNS = "";
        Endpoint = "";

        ListenPort = 51820;
        NumberOfClients = 3;

        Subnet = "10.0.0.0/24";
        AllowedIPs = "0.0.0.0/0, ::/0";

        UseLastAddress = false;
        UsePresharedKeys = false;

        PostUp = "";
        PostDown = "";

        CommandOnce = "";
        CommandPerPeer = "";
        CommandFileName = "output.wgc";
    }
}
