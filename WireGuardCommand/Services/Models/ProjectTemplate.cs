namespace WireGuardCommand.Services.Models;

public class ProjectTemplate
{
    public string Name { get; init; } = "Default";

    public string Interface { get; init; } = "wg0";

    public int ListenPort { get; init; } = 51820;
    public int NumberOfClients { get; init; } = 3;

    public string DNS { get; init; } = "";
    public string Endpoint { get; init; } = "";

    public string Subnet { get; init; } = "10.0.0.0/24";
    public string AllowedIPs { get; init; } = "0.0.0.0/0, ::/0";

    public bool UseLastAddress { get; init; }
    public bool UsePresharedKeys { get; init; }

    public string PostUp { get; init; } = "";
    public string PostDown { get; init; } = "";

    public string CommandOnce { get; init; } = "";
    public string CommandPerPeer { get; init; } = "";
    public string CommandFileName { get; init; } = "output.wgc";
}
