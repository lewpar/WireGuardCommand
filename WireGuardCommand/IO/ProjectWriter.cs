using System.Net;
using System.Text;

using WireGuardCommand.Extensions;
using WireGuardCommand.Models;
using WireGuardCommand.Security;
using WireGuardCommand.Services.Models;

namespace WireGuardCommand.IO;

public class ProjectWriter
{
    private readonly ProjectData project;

    public ProjectWriter(ProjectData project)
    {
        this.project = project;
    }

    private List<WireGuardPeer> GeneratePeers()
    {
        if (!project.Subnet.TryParseAddress(out IPNetwork2 subnet))
        {
            throw new Exception("Invalid subnet.");
        }

        int peerId = 1;
        var peers = new List<WireGuardPeer>();

        foreach (var address in subnet.ListIPAddress(FilterEnum.Usable))
        {
            // + 1 to account for server peer.
            if (peerId > project.NumberOfClients + 1)
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

    public async Task WriteConfigsAsync(string path)
    {
        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }

        var peers = GeneratePeers();

        // Take first/last peer out of the list to use as server.
        var serverPeer = project.UseLastAddress ? peers.Last() : peers.First();
        peers = project.UseLastAddress ? peers.Take(peers.Count - 1).ToList() : peers.Skip(1).ToList();

        var server = new StringBuilder();

        server.AppendLine("[Interface]");
        server.AppendLine($"Address = {serverPeer.Address}/{serverPeer.Subnet.Cidr}");
        server.AppendLine($"ListenPort = {project.ListenPort}");
        server.AppendLine($"PrivateKey = {serverPeer.PrivateKey}");
        server.AppendLine();

        foreach (var peer in peers)
        {
            CurveKeypair? presharedKey = null;
            int peerId = project.UseLastAddress ? peer.Id : peer.Id - 1;

            if (project.UsePresharedKeys)
            {
                presharedKey = new CurveKeypair();
            }

            server.AppendLine($"# Peer {peerId}");
            server.AppendLine("[Peer]");
            server.AppendLine($"PublicKey = {peer.PublicKey}");

            if (presharedKey is not null)
            {
                server.AppendLine($"PresharedKey = {presharedKey.PrivateKey}");
            }

            server.AppendLine($"AllowedIPs = {peer.Address}/32");
            server.AppendLine();

            var client = new StringBuilder();

            client.AppendLine($"# Peer {peerId}");
            client.AppendLine("[Interface]");
            client.AppendLine($"Address = {peer.Address}/{peer.Subnet.Cidr}");
            client.AppendLine($"ListenPort = {peer.Port}");
            client.AppendLine($"PrivateKey = {peer.PrivateKey}");

            if (!string.IsNullOrWhiteSpace(peer.DNS))
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

            if (!string.IsNullOrWhiteSpace(peer.Endpoint))
            {
                client.AppendLine($"Endpoint = {peer.Endpoint}");
            }

            await File.WriteAllTextAsync(Path.Combine(path, $"peer-{peerId}.conf"), client.ToString());
        }

        await File.WriteAllTextAsync(Path.Combine(path, "server.conf"), server.ToString());
    }
}
