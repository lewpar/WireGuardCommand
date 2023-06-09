using System.Collections.Generic;

namespace WireGuardCommand.Models
{
    public class WireGuardServer
    {
        public string? Address { get; set; }
        public int Port { get; set; }
        public string? PrivateKey { get; set; }
        public string? PublicKey { get; set; }
        public string? Interface { get; set; }

        public List<WireGuardPeer> Peers { get; set; }
        public List<string> Commands { get; set; }

        public WireGuardServer()
        {
            Peers = new List<WireGuardPeer>();
            Commands = new List<string>();
        }
    }
}
