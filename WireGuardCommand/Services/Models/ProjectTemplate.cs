namespace WireGuardCommand.Services.Models;

public class ProjectTemplate
{
    public string Name { get; set; }

    public int ListenPort { get; set; }
    public int NumberOfClients { get; set; }

    public string Subnet { get; set; }
    public string AllowedIPs { get; set; }

    public ProjectTemplate()
    {
        Name = "Default";

        ListenPort = 51820;
        NumberOfClients = 3;

        Subnet = "10.0.0.0/24";
        AllowedIPs = "0.0.0.0/0, ::/0";
    }
}
