using System;
using System.Diagnostics;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace WireGuardCommand.Security
{
    public static class AESEncryption
    {
        public static byte[] Encrypt(byte[] data, byte[] key, byte[] iv)
        {
            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.Key = key;
                aesAlg.IV = iv;

                ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);

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

        public static byte[] GenerateIV()
        {
            using(Aes aes = Aes.Create())
            {
                aes.GenerateIV();

                return aes.IV;
            }
        }

        /// <summary>
        /// Creates a cryptographically secure key using a passphrase and returns the key and salt.
        /// </summary>
        /// <param name="passphrase">The passphrase to generate the key with.</param>
        /// <param name="iterations">How many iterations to derive the key from.</param>
        /// <param name="salt">The salt which is appending onto the passphrase during encryption.</param>
        /// <returns>(Key, Salt)</returns>
        public static (byte[], byte[]) GenerateKey(string passphrase, byte[]? salt = null, int iterations = 10000)
        {
            var key = Encoding.UTF8.GetBytes(passphrase);

            if(salt is null)
            {
                salt = RandomNumberGenerator.GetBytes(32);
            }

            using (var deriveBytes = new Rfc2898DeriveBytes(key, salt, iterations))
            {
                return (deriveBytes.GetBytes(256 / 8), salt);
            }
        }

        public static bool TryDecrypt(byte[] data, byte[] key, byte[] iv, out byte[]? output)
        {
            try
            {
                using (Aes aesAlg = Aes.Create())
                {
                    aesAlg.Key = key;
                    aesAlg.IV = iv;

                    ICryptoTransform decryptor = aesAlg.CreateDecryptor(key, iv);

                    using (MemoryStream msDecrypt = new MemoryStream(data))
                    {
                        using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                        {
                            if (!csDecrypt.CanRead)
                            {
                                output = null;
                                return false;
                            }

                            var msOutput = new MemoryStream();
                            csDecrypt.CopyTo(msOutput);

                            output = msOutput.ToArray();
                            return true;
                        }
                    }
                }
            }
            catch(Exception ex)
            {
                Debug.WriteLine($"Failed to decrypt project with error: {ex.Message}.");
                output = null;
                return false;
            }
        }
    }
}
