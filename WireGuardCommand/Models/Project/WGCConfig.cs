using CommunityToolkit.Mvvm.ComponentModel;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;

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
        private int maxNoOfClients;

        private string? cidr;
        public string? Cidr
        {
            get => cidr;
            set
            {
                if(cidr != value)
                {
                    cidr = value;
                    SetProperty(ref cidr, value);

                    MaxNoOfClients = GetUsableHostsCount();

                    // Re-trigger input validation.
                    OnPropertyChanged(nameof(NoOfClients)); // TODO: The field validation errors are not being triggered, look into.
                }
            }
        }

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

        private CurveKeyPair? serverKeyPair;
        private Dictionary<int, CurveKeyPair> clientKeyPairs;

        public WGCConfig()
        {
            ListenPort = 51820;

            Cidr = "10.0.0.0/24";
            NoOfClients = 1;
            MaxNoOfClients = GetUsableHostsCount();

            AllowedIPs = "0.0.0.0/0, ::/0";
            Endpoint = string.Empty;
            Dns = string.Empty;
            PostUpRule = "iptables -A FORWARD -i %i -j ACCEPT; iptables -t nat -A POSTROUTING -o eth0 -j MASQUERADE";
            PostDownRule = "iptables -D FORWARD -i %i -j ACCEPT; iptables -t nat -D POSTROUTING -o eth0 -j MASQUERADE";

            Seed = RandomHelper.GetRandomSeed();

            clientKeyPairs = new Dictionary<int, CurveKeyPair>();
            serverKeyPair = new CurveKeyPair();
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

            sb.AppendLine($"PrivateKey = {serverKeyPair?.PrivateKey}");

            if (!string.IsNullOrEmpty(PostUpRule))
            {
                sb.AppendLine($"PostUp = {PostUpRule}");
            }

            if (!string.IsNullOrEmpty(PostDownRule))
            {
                sb.AppendLine($"PostDown = {PostDownRule}");
            }

            sb.AppendLine();

            // Generate peers section
            for(int i = 1; i < NoOfClients + 1; i++)
            {
                sb.AppendLine($"# Client No {i}");
                sb.AppendLine("[Peer]");
                sb.AppendLine($"PublicKey = {clientKeyPairs[i].PublicKey}");
                sb.AppendLine($"AllowedIPs = {AllowedIPs}");
                sb.AppendLine();
            }

            return sb.ToString();
        }

        public string? GenerateClient(int clientId)
        {
            var sb = new StringBuilder();

            sb.AppendLine("[Interface]");
            sb.AppendLine($"Address = 10.0.0.{clientId}/24"); // TODO: Replace with proper host address.
            sb.AppendLine($"ListenPort = {ListenPort}");
            sb.AppendLine($"PrivateKey = {clientKeyPairs[clientId].PrivateKey}");

            sb.AppendLine();

            sb.AppendLine("[Peer]");
            sb.AppendLine($"PublicKey = {serverKeyPair?.PublicKey}");
            sb.AppendLine($"AllowedIPs = {AllowedIPs}");

            if(!string.IsNullOrEmpty(Endpoint))
            {
                sb.AppendLine($"Endpoint = {Endpoint}");
            }

            return sb.ToString();
        }

        public void GenerateKeyPairs()
        {
            serverKeyPair = string.IsNullOrEmpty(Seed) ? 
                CurveKeyPair.GeneratePair() : 
                CurveKeyPair.GeneratePair(Convert.FromBase64String(Seed));

            clientKeyPairs.Clear();
            for(int i = 1; i < NoOfClients + 1; i++)
            {
                var clientKeyPair = string.IsNullOrEmpty(Seed) ? 
                    CurveKeyPair.GeneratePair() : 
                    CurveKeyPair.GeneratePair(Convert.FromBase64String(Seed), i);

                clientKeyPairs.Add(i, clientKeyPair);
            }
        }

        public int GetUsableHostsCount()
        {
            if(IPNetwork.TryParse(Cidr, out IPNetwork ipNetwork))
            {
                return (int)ipNetwork.Usable;
            }

            return 0;
        }
    }
}
