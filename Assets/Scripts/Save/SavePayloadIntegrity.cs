using System;
using System.Security.Cryptography;
using System.Text;
using NullPointer.Core;
using UnityEngine;

namespace NullPointer.Save
{
    internal static class SavePayloadIntegrity
    {
        public static string Compute(GameStateSnapshot snapshot)
        {
            if (snapshot == null)
            {
                throw new ArgumentNullException(nameof(snapshot));
            }

            byte[] bytes = Encoding.UTF8.GetBytes(JsonUtility.ToJson(snapshot, false));
            using SHA256 sha256 = SHA256.Create();
            byte[] hash = sha256.ComputeHash(bytes);
            var builder = new StringBuilder(hash.Length * 2);
            foreach (byte value in hash)
            {
                builder.Append(value.ToString("x2"));
            }

            return builder.ToString();
        }

        public static bool Matches(GameStateSnapshot snapshot, string expectedHash)
        {
            if (string.IsNullOrWhiteSpace(expectedHash))
            {
                return false;
            }

            byte[] expected = Encoding.UTF8.GetBytes(expectedHash.Trim().ToLowerInvariant());
            byte[] actual = Encoding.UTF8.GetBytes(Compute(snapshot));
            return expected.Length == actual.Length && CryptographicOperations.FixedTimeEquals(expected, actual);
        }
    }
}
