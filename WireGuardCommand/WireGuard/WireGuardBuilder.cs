﻿using System.Net;
using System.Security.Cryptography;
using WireGuardCommand.Security;

namespace WireGuardCommand.WireGuard;

public class WireGuardBuilder
{
    private int currentPeerId;

    private readonly WireGuardPeer server;
    private readonly List<WireGuardPeer> peers;

    private readonly WireGuardBuilderOptions options;

    public WireGuardBuilder(WireGuardBuilderOptions options)
    {
        this.options = options;

        currentPeerId = 1;

        var keypair = new CurveKeypair(GetPeerSeed(options.Seed, 0));
        server = new WireGuardPeer()
        {
            Id = 0,

            Role = WireGuardPeerRole.Server,

            Subnet = options.Subnet,
            Address = options.UseLastAddress ? 
                        options.Subnet.LastUsable : 
                        options.Subnet.FirstUsable,
            ListenPort = options.ListenPort,
            Endpoint = options.Endpoint,

            PostUp = options.PostUp,
            PostDown = options.PostDown,

            PrivateKey = keypair.PrivateKey,
            PublicKey = keypair.PublicKey
        };

        peers = new List<WireGuardPeer>();
        server.Peers = peers;
    }

    public WireGuardBuilder AddPeer()
    {
        var subnet = options.Subnet;

        var peerCount = peers.Count + 1;
        if (peerCount > subnet.Usable)
        {
            throw new Exception($"There is not enough space in this subnet for '{peerCount}' peers.");
        }

        var address = options.UseLastAddress ? 
            subnet.ListIPAddress(FilterEnum.Usable).Skip(currentPeerId - 1).First() : 
            subnet.ListIPAddress(FilterEnum.Usable).Skip(currentPeerId).First();

        var keypair = new CurveKeypair(GetPeerSeed(options.Seed, currentPeerId));
        var presharedKeypair = new CurveKeypair(GetPeerSeed(options.Seed, currentPeerId).Reverse().ToArray());

        peers.Add(new WireGuardPeer()
        {
            Id = currentPeerId,

            Role = WireGuardPeerRole.Client,

            Address = address,
            Subnet = options.Subnet,
            ListenPort = options.ListenPort,
            DNS = options.DNS,

            PrivateKey = keypair.PrivateKey,
            PublicKey = keypair.PublicKey,

            PresharedKey = options.UsePresharedKeys ? presharedKeypair.PrivateKey : null,

            AllowedIPs = options.AllowedIPs,

            Peers = new List<WireGuardPeer>()
            {
                server
            }
        });

        currentPeerId++;

        return this;
    }

    public WireGuardBuilder AddPeers(int count)
    {
        for (int i = 0; i < count; i++)
        {
            AddPeer();
        }

        return this;
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

    public WireGuardPeer Build()
    {
        return server;
    }
}
