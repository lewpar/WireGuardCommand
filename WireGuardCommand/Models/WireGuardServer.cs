using System.Collections.Generic;
using System.Net;

namespace WireGuardCommand.Models
{
    public static class WireGuardServer
    {
        public static string? Address { get; set; }
        public static IPNetwork? Subnet { get; set; }
        public static int Port { get; set; }
        public static string? PrivateKey { get; set; }
        public static string? PublicKey { get; set; }
        public static string? Interface { get; set; }

        public static List<WireGuardPeer> Peers { get; set; } = new List<WireGuardPeer>();
        public static List<string> Commands { get; set; } = new List<string>();

        public static void Reset()
        {
            Address = default;
            Subnet = default;
            Port = default;
            PrivateKey = default;
            PublicKey = default;
            Interface = default;

            Peers.Clear();
            Commands.Clear();
        }
    }
}
