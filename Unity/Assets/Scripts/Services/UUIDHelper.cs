using System;
using System.Security.Cryptography;
using System.Text;

namespace EpubParser.Services
{
    internal class UUIDHelper
    {
        public static Guid GenerateDeterministicGuid(string value)
        {
            byte[] input = Encoding.UTF8.GetBytes(value);
            byte[] hash = SHA256.Create().ComputeHash(input);
            byte[] guidBytes = new byte[16];
            Array.Copy(hash, guidBytes, 16);
            return new Guid(guidBytes);
        }
    }
}
