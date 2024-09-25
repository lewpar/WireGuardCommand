namespace WireGuardCommand.Services.Models;

public class WireGuardClientConfig
{
    public int ListenPort { get; set; }

    public string Gateway { get; set; }
    public string AllowedIPs { get; set; }
    public string DNS { get; set; }

    public WireGuardClientConfig()
    {
        ListenPort = 51820;

        Gateway = "10.0.0.1";
        AllowedIPs = "0.0.0.0/0, ::/0";
        DNS = "10.0.100.254";
    }
}
