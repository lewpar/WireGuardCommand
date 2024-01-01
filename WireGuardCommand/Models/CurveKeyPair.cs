using WireGuardCommand.Security;

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

            return GeneratePair(buffer);
        }

        public static CurveKeyPair GeneratePair(byte[] seed)
        {
            byte[] privKey = Curve25519.ClampPrivateKey(seed);
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
