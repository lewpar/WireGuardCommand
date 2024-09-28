using System.Text;

using WireGuardCommand.Models;

namespace WireGuardCommand.IO;

public class WireGuardWriter
{
    private string GenerateConfig(WireGuardPeer client)
    {
        var sb = new StringBuilder();
        bool isServer = client.Role == WireGuardPeerRole.Server;

        if (isServer)
        {
            sb.AppendLine($"# Server");
        }
        else
        {
            sb.AppendLine($"# Peer {client.Id}");
        }

        sb.AppendLine("[Interface]");

        sb.AppendLine($"Address = {client.Address}/{client.Subnet.Cidr}");
        sb.AppendLine($"ListenPort = {client.ListenPort}");
        sb.AppendLine($"PrivateKey = {client.PrivateKey}");
        sb.AppendLine();

        foreach (var peer in client.Peers)
        {
            if (peer.Role == WireGuardPeerRole.Server)
            {
                sb.AppendLine($"# Server");
            }
            else
            {
                sb.AppendLine($"# Peer {peer.Id}");
            }
            sb.AppendLine("[Peer]");
            sb.AppendLine($"PublicKey = {peer.PublicKey}");

            if (isServer && peer.PresharedKey is not null)
            {
                sb.AppendLine($"PresharedKey = {peer.PresharedKey}");
            }
            else if (!isServer && client.PresharedKey is not null)
            {
                sb.AppendLine($"PresharedKey = {client.PresharedKey}");
            }

            if (isServer)
            {
                sb.AppendLine($"AllowedIPs = {peer.Address}/32");
            }
            else
            {
                sb.Append($"AllowedIPs = {peer.AllowedIPs}");
            }

            sb.AppendLine();
        }

        return sb.ToString();
    }

    public void Write(WireGuardPeer client, Stream outputStream)
    {
        var data = Encoding.UTF8.GetBytes(GenerateConfig(client));
        outputStream.Write(data, 0, data.Length);
    }

    public async Task WriteAsync(WireGuardPeer client, Stream outputStream)
    {
        var data = Encoding.UTF8.GetBytes(GenerateConfig(client));
        await outputStream.WriteAsync(data, 0, data.Length);
    }
}
