using ICSharpCode.SharpZipLib.Zip;

using System.Net;
using System.Security.Cryptography;
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

    private byte[] GetPeerSeed(byte[] seed, int peerId)
    {
        using var sha256 = SHA256.Create();

        byte[] dataIdBytes = BitConverter.GetBytes(peerId);

        byte[] combined = new byte[seed.Length + dataIdBytes.Length];
        Buffer.BlockCopy(seed, 0, combined, 0, seed.Length);
        Buffer.BlockCopy(dataIdBytes, 0, combined, seed.Length, dataIdBytes.Length);

        return sha256.ComputeHash(combined);
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

            var keypair = new CurveKeypair(GetPeerSeed(project.Seed.FromBase64(), peerId));

            var peer = new WireGuardPeer()
            {
                Id = peerId,

                Subnet = subnet,
                Address = address,
                ListenPort = project.ListenPort,

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
                // Reverse the seed so it is different from the main seed and generates 
                // different keys.
                var reversedSeed = GetPeerSeed(project.Seed.FromBase64(), peerId).Reverse().ToArray();

                presharedKey = new CurveKeypair(reversedSeed);
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
            client.AppendLine($"ListenPort = {peer.ListenPort}");
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

        if(project.IsZippedOutput)
        {
            using var fs = File.OpenWrite(Path.Combine(path, "output.zip"));
            using var zipStream = new ZipOutputStream(fs);

            bool hasPassword = !string.IsNullOrWhiteSpace(project.ZipPassphrase);

            if (hasPassword)
            {
                zipStream.Password = project.ZipPassphrase;
            }

            var files = Directory.GetFiles(path, "*.conf", SearchOption.AllDirectories);
            foreach(var file in files)
            {
                var fileName = Path.GetFileName(file);

                var newEntry = new ZipEntry(fileName)
                {
                    DateTime = DateTime.Now
                };

                zipStream.PutNextEntry(newEntry);

                using (var configStream = File.OpenRead(file))
                {
                    await configStream.CopyToAsync(zipStream);
                    zipStream.CloseEntry();
                }

                File.Delete(file);
            }
        }
    }
}
