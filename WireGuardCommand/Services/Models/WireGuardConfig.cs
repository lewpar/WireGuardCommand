using System.Net;
using System.Text;

using WireGuardCommand.Extensions;
using WireGuardCommand.Security;

namespace WireGuardCommand.Services.Models;

public class WireGuardConfig
{
    private readonly ProjectData project;

    public WireGuardConfig(ProjectData project)
    {
        this.project = project;
    }

    public List<WireGuardPeer> GetPeers()
    {
        if (!TryParseAddress(project.Subnet, out IPNetwork2 subnet))
        {
            throw new Exception("Invalid subnet.");
        }

        int peerId = 1;
        var peers = new List<WireGuardPeer>();

        foreach(var address in subnet.ListIPAddress(FilterEnum.Usable))
        {
            // + 1 to account for server peer.
            if(peerId > project.NumberOfClients + 1)
            {
                break;
            }

            var keypair = new CurveKeypair(project.Seed.FromBase64());

            var peer = new WireGuardPeer()
            {
                Id = peerId,

                Subnet = subnet,
                Address = address,
                Port = project.ListenPort,

                AllowedIPs = project.AllowedIPs,

                PublicKey = keypair.PublicKey,
                PrivateKey = keypair.PrivateKey,

                DNS = project.DNS,
                Endpoint = project.Endpoint
            };

            peers.Add(peer);

            peerId++;
        }

        return peers;
    }

    public void Generate(string path)
    {
        if(!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }

        var peers = GetPeers();

        // Take first peer out of the list to use as server.
        var serverPeer = peers.First();
        peers = peers.Skip(1).ToList();

        var server = new StringBuilder();

        server.AppendLine("[Interface]");
        server.AppendLine($"Address = {serverPeer.Address}/{serverPeer.Subnet.Cidr}");
        server.AppendLine($"ListenPort = {project.ListenPort}");
        server.AppendLine($"PrivateKey = {serverPeer.PrivateKey}");
        server.AppendLine();

        foreach(var peer in peers) 
        {
            CurveKeypair? presharedKey = null;
            int peerId = peer.Id - 1;

            if (project.UsePresharedKeys)
            {
                presharedKey = new CurveKeypair();
            }

            server.AppendLine($"# Peer {peerId}");
            server.AppendLine("[Peer]");
            server.AppendLine($"PublicKey = {peer.PublicKey}");

            if(presharedKey is not null)
            {
                server.AppendLine($"PresharedKey = {presharedKey.PrivateKey}");
            }

            server.AppendLine($"AllowedIPs = {peer.Address}/32");
            server.AppendLine();

            var client = new StringBuilder();

            client.AppendLine($"# Peer {peerId}");
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

            if (presharedKey is not null)
            {
                client.AppendLine($"PresharedKey = {presharedKey.PrivateKey}");
            }

            client.AppendLine($"AllowedIPs = {peer.AllowedIPs}");

            if(!string.IsNullOrWhiteSpace(peer.Endpoint))
            {
                client.AppendLine($"Endpoint = {peer.Endpoint}");
            }


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
