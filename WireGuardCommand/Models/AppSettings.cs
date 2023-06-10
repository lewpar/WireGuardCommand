using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace WireGuardCommand.Models
{
    public class AppSettings
    {
        public string? DefaultPrefix { get; set; }
        public string? DefaultCommand { get; set; }
        public string? DefaultPostfix { get; set; }

        public int? DefaultListenPort { get; set; }
        public int? DefaultNoClients { get; set; }
        public string? DefaultSubnet { get; set; }

        public string? DefaultIPs { get; set; }
        public string? DefaultEndpoint { get; set; }
        public string? DefaultDNS { get; set; }

        public string? DefaultInterface { get; set; }
        public bool? DefaultPresharedKeys { get; set; }

        public string? SaveLocation { get; set; }

        public void Save()
        {
            string json = JsonSerializer.Serialize(this);
            File.WriteAllText(@"./appsettings.json", json);
        }

        public void Reset()
        {
            DefaultPrefix = "/interface wireguard\r\nadd listen-port={listenport} name={interface} private-key\"{server-private}\"\r\n\r\n/ip address\r\nadd address={server-address} interface={interface}\r\n\r\n/interface wireguard peers";
            DefaultCommand = "add allowed-address={client-address} comment=\"peer {client-id}\" interface=\"{interface}\" preshared-key=\"{client-preshared}\" public-key=\"{client-public}\"";
            DefaultPostfix = string.Empty;

            DefaultListenPort = 1234;
            DefaultNoClients = 3;
            DefaultSubnet = "10.0.0.0/24";

            DefaultIPs = "0.0.0.0/0";
            DefaultEndpoint = "remote.domain.com:1234";
            DefaultDNS = "10.0.100.254";

            DefaultInterface = "WireGuardInterface1";
            DefaultPresharedKeys = true;

            SaveLocation = string.Empty;
        }
    }
}
