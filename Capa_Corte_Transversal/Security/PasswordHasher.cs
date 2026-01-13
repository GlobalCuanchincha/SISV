using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;

namespace Capa_Corte_Transversal.Security
{
    public static class PasswordHasher
    {
        public static void CreateHash(
            string password,
            out byte[] hash,
            out byte[] salt,
            int iterations = 10000)
        {
            using (var rng = new RNGCryptoServiceProvider())
            {
                salt = new byte[32];
                rng.GetBytes(salt);
            }

            using (var pbkdf2 = new Rfc2898DeriveBytes(password, salt, iterations))
            {
                hash = pbkdf2.GetBytes(64);
            }
        }

        public static bool Verify(
            string password,
            byte[] storedHash,
            byte[] storedSalt,
            int iterations)
        {
            if (storedHash == null || storedSalt == null)
                return false;

            byte[] testHash;

            using (var pbkdf2 = new Rfc2898DeriveBytes(password, storedSalt, iterations))
            {
                testHash = pbkdf2.GetBytes(64);
            }

            return FixedTimeEquals(testHash, storedHash);
        }

        private static bool FixedTimeEquals(byte[] a, byte[] b)
        {
            if (a == null || b == null || a.Length != b.Length)
                return false;

            int diff = 0;

            for (int i = 0; i < a.Length; i++)
            {
                diff |= a[i] ^ b[i];
            }

            return diff == 0;
        }
    }
}

