using System.Security.Cryptography;

namespace WireGuardCommand.Security;

public class AESProvider
{
    public static byte[] GenerateIV()
    {
        using var aes = Aes.Create();
        aes.GenerateIV();

        return aes.IV;
    }

    public static byte[] GenerateKey(string passphrase, byte[] salt)
    {
        using var derive = new Rfc2898DeriveBytes(passphrase, salt, 10000, HashAlgorithmName.SHA256);

        return derive.GetBytes(32);
    }

    public static byte[] Encrypt(byte[] data, byte[] key, byte[] iv)
    {
        using (var aes = Aes.Create())
        {
            aes.Key = key;
            aes.IV = iv;

            ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV);

            using (MemoryStream msEncrypt = new MemoryStream())
            {
                using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                {
                    csEncrypt.Write(data, 0, data.Length);
                }

                return msEncrypt.ToArray();
            }
        }
    }

    public static byte[]? Decrypt(FileStream file, byte[] key, byte[] iv)
    {
        using (Aes aesAlg = Aes.Create())
        {
            aesAlg.Key = key;
            aesAlg.IV = iv;

            ICryptoTransform decryptor = aesAlg.CreateDecryptor(key, iv);

            using (CryptoStream csDecrypt = new CryptoStream(file, decryptor, CryptoStreamMode.Read))
            {
                if (!csDecrypt.CanRead)
                {
                    return null;
                }

                var msOutput = new MemoryStream();
                csDecrypt.CopyTo(msOutput);

                return msOutput.ToArray();
            }
        }
    }
}
