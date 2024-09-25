using System.Security.Cryptography;

namespace WireGuardCommand.Security;

public class CurveKeypair
{
    public CurveKey PublicKey { get; set; }
    public CurveKey PrivateKey { get; set; }

    public CurveKeypair(byte[]? seed = null, int size = 32)
    {
        if(seed is null)
        {
            seed = new byte[size];
            RandomNumberGenerator.Create().GetBytes(seed);
        }

        byte[] privKey = Curve25519.ClampPrivateKey(seed);
        byte[] pubKey = Curve25519.GetPublicKey(privKey);

        PublicKey = new CurveKey()
        {
            Key = pubKey
        };

        PrivateKey = new CurveKey()
        {
            Key = privKey
        };
    }
}
