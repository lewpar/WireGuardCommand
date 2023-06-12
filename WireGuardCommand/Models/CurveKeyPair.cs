using Elliptic;
using System.Security.Cryptography;

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

            byte[] privKey = Curve25519.ClampPrivateKey(buffer);
            byte[] pubKey = Curve25519.GetPublicKey(buffer);


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
