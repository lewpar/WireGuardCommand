using CommunityToolkit.Mvvm.ComponentModel;

using WireGuardCommand.Security;

namespace WireGuardCommand.Models.Project
{
    public partial class WGCConfig : ObservableObject
    {
        [ObservableProperty]
        private int listenPort;

        [ObservableProperty]
        private int noOfClients;

        [ObservableProperty]
        private string? cidr;

        [ObservableProperty]
        private string? seed;

        [ObservableProperty]
        private string? allowedIPs;

        [ObservableProperty]
        private string? endpoint;

        [ObservableProperty]
        private string? dns;

        [ObservableProperty]
        private string? postUpRule;

        [ObservableProperty]
        private string? postDownRule;

        public WGCConfig()
        {
            ListenPort = 51820;
            NoOfClients = 1;
            Cidr = "10.0.0.0/24";
            AllowedIPs = "0.0.0.0/0, ::/0";
            Endpoint = string.Empty;
            Dns = string.Empty;
            PostUpRule = "iptables -A FORWARD -i %i -j ACCEPT; iptables -t nat -A POSTROUTING -o eth0 -j MASQUERADE";
            PostDownRule = "iptables -D FORWARD -i %i -j ACCEPT; iptables -t nat -D POSTROUTING -o eth0 -j MASQUERADE";

            Seed = RandomHelper.GetRandomSeed();
        }
    }
}
