using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Text;

using WireGuardCommand.Security;

namespace WireGuardCommand.Services.Models;

public class WireGuardConfig
{
    public WireGuardServerConfig Server { get; set; }
    public WireGuardClientConfig Client { get; set; }

    public string Interface { get; set; }

    public int NoOfClients { get; set; }
    public string Subnet { get; set; }

    public bool UsePresharedKeys { get; set; }

    public WireGuardConfig()
    {
        Server = new WireGuardServerConfig();
        Client = new WireGuardClientConfig();

        Interface = "WireGuardInterface1";

        NoOfClients = 3;
        Subnet = "10.0.0.0/24";

        UsePresharedKeys = false;
    }

    public List<WireGuardPeer> GetPeers(byte[] seed)
    {
        if (!TryParseAddress(Subnet, out IPNetwork2 subnet))
        {
            throw new Exception("Invalid subnet.");
        }

        int peerId = 1;
        var peers = new List<WireGuardPeer>();

        foreach(var address in subnet.ListIPAddress(FilterEnum.Usable))
        {
            // + 1 to account for server peer.
            if(peerId > NoOfClients + 1)
            {
                break;
            }

            var keypair = new CurveKeypair(seed);

            var peer = new WireGuardPeer()
            {
                Id = peerId,

                Subnet = subnet,
                Address = address,
                Port = Server.ListenPort,

                AllowedIPs = Client.AllowedIPs,

                PublicKey = keypair.PublicKey,
                PrivateKey = keypair.PrivateKey,

                DNS = Client.DNS,
                Endpoint = Server.Endpoint
            };

            peers.Add(peer);

            peerId++;
        }

        return peers;
    }

    public void Generate(string path, byte[] seed)
    {
        if(!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }

        var peers = GetPeers(seed);

        // Take first peer out of the list to use as server.
        var serverPeer = peers.First();
        peers = peers.Skip(1).ToList();

        var server = new StringBuilder();

        server.AppendLine("[Interface]");
        server.AppendLine($"Address = {serverPeer.Address}/{serverPeer.Subnet.Cidr}");
        server.AppendLine($"ListenPort = {Server.ListenPort}");
        server.AppendLine($"PrivateKey = {serverPeer.PrivateKey}");
        server.AppendLine();

        foreach(var peer in peers) 
        {
            server.AppendLine("[Peer]");
            server.AppendLine($"PublicKey = {peer.PublicKey}");

            if(UsePresharedKeys)
            {
                server.AppendLine("PresharedKey = TODO");
            }

            server.AppendLine($"AllowedIPs = {peer.Address}/32");
            server.AppendLine();


            int peerId = peer.Id - 1;

            var client = new StringBuilder();

            client.AppendLine($"# Client {peerId}");
            client.AppendLine("[Interface]");
            client.AppendLine($"Address = {peer.Address}/{ peer.Subnet.Cidr}");
            client.AppendLine($"ListenPort = {peer.Port}");
            client.AppendLine($"PrivateKey = {peer.PrivateKey}");

            if(!string.IsNullOrWhiteSpace(peer.DNS))
            {
                client.AppendLine($"DNS = {peer.DNS}");
            }

            client.AppendLine();

            client.AppendLine("[Peer]");
            client.AppendLine($"PublicKey = {serverPeer.PublicKey}");

            if(UsePresharedKeys)
            {
                client.AppendLine($"PresharedKey = TODO");
            }

            client.AppendLine($"AllowedIPs = {peer.AllowedIPs}");
            client.AppendLine($"Endpoint = {peer.Endpoint}");


            File.WriteAllText(Path.Combine(path, $"peer-{peerId}.conf"), client.ToString());
        }

        File.WriteAllText(Path.Combine(path, "server.conf"), server.ToString());
    }

    private static bool TryParseAddress(string address, out IPNetwork2 result)
    {
        try
        {
            result = IPNetwork2.Parse(address);
            return true;
        }
        catch
        {
            result = new IPNetwork2();
            return false;
        }
    }
}
