using WireGuardCommand.Security;

using System.Security.Cryptography;
using System;

namespace WireGuardCommand.Models
{
    public class CurveKeyPair
    {
        public CurveKey? PublicKey { get; set; }
        public CurveKey? PrivateKey { get; set; }

        public static CurveKeyPair GeneratePair(int size = 32)
        {
            byte[] buffer = new byte[size];
            RandomNumberGenerator.Create().GetBytes(buffer);

            return GeneratePair(buffer);
        }

        public static CurveKeyPair GeneratePair(byte[] seed, int peerId = 0)
        {
            byte[] peerIdBuf = BitConverter.GetBytes(peerId);
            byte[] buffer = new byte[seed.Length + peerIdBuf.Length];

            // Combine the seed and peer id to create a unique but consistent seed.
            Array.Copy(seed, buffer, seed.Length);
            Array.Copy(peerIdBuf, 0, buffer, seed.Length, peerIdBuf.Length);

            // Resize the seed back to 32 bytes.
            using(SHA256 sha = SHA256.Create())
            {
                buffer = sha.ComputeHash(buffer);
            }

            byte[] privKey = Curve25519.ClampPrivateKey(buffer);
            byte[] pubKey = Curve25519.GetPublicKey(privKey);

            return new CurveKeyPair()
            {
                PublicKey = new CurveKey()
                {
                    Key = pubKey
                },
                PrivateKey = new CurveKey()
                {
                    Key = privKey
                }
            };
        }
    }
}
