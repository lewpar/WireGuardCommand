namespace WireGuardCommand.Services.Models;

public class WireGuardServerConfig
{
    public int ListenPort { get; set; }
    public string Endpoint { get; set; }

    public WireGuardServerConfig()
    {
        ListenPort = 51820;
        Endpoint = "remote.domain.com:51820";
    }
}
