using CommunityToolkit.Mvvm.ComponentModel;

using System;
using System.Security.Cryptography;
using System.Text;
using System.Windows.Documents;
using WireGuardCommand.Security;

namespace WireGuardCommand.Models.Project
{
    public partial class WGCConfig : ObservableObject, IEquatable<WGCConfig>
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

        public bool Equals(WGCConfig? other)
        {
            if(other is null)
            {
                return false;
            }

            return this.ListenPort == other.ListenPort &&
                this.NoOfClients == other.NoOfClients &&
                this.Cidr == other.Cidr &&
                this.Seed == other.Seed &&
                this.AllowedIPs == other.AllowedIPs &&
                this.Endpoint == other.Endpoint &&
                this.Dns == other.Dns &&
                this.PostUpRule == other.PostUpRule &&
                this.PostDownRule == other.PostDownRule;
        }

        public string? GenerateServer()
        {
            var sb = new StringBuilder();

            sb.AppendLine("[Interface]");
            sb.AppendLine($"Address = {Cidr}"); // TODO: This should not be the cidr but the first / last address in usable addresses.
            sb.AppendLine($"ListenPort = {ListenPort}");
            sb.AppendLine($"PrivateKey = {Convert.ToBase64String(RandomNumberGenerator.GetBytes(32))}"); // TODO: This should be a private key and not just random bytes.

            if(!string.IsNullOrEmpty(PostUpRule))
            {
                sb.AppendLine($"PostUp = {PostUpRule}");
            }

            if (!string.IsNullOrEmpty(PostDownRule))
            {
                sb.AppendLine($"PostDown = {PostDownRule}");
            }

            sb.AppendLine();

            // Generate peers section
            for(int i = 0; i < NoOfClients; i++)
            {
                sb.AppendLine("[Peer]");
                sb.AppendLine($"PublicKey = {Convert.ToBase64String(RandomNumberGenerator.GetBytes(32))}"); // TODO: This should be a public key and not just random bytes.
                sb.AppendLine($"AllowedIPs = {AllowedIPs}");
            }

            return sb.ToString();
        }
    }
}
