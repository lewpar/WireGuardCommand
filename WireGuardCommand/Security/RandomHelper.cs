using System;
using System.Security.Cryptography;

namespace WireGuardCommand.Security
{
    public static class RandomHelper
    {
        public static string GetRandomSeed()
        {
            byte[] buffer = new byte[32];
            RandomNumberGenerator.Fill(buffer);

            return Convert.ToBase64String(buffer);
        }
    }
}
