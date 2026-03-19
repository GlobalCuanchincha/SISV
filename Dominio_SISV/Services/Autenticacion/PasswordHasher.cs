using System;
using System.Security.Cryptography;

namespace Capa_Corte_Transversal.Security
{
    public static class PasswordHasher
    {
        public static (byte[] Hash, byte[] Salt, int Iterations) Create(string password, int iterations = 100000, int saltSize = 16, int hashSize = 32)
        {
            if (password == null) throw new ArgumentNullException(nameof(password));
            if (iterations <= 0) throw new ArgumentOutOfRangeException(nameof(iterations));

            byte[] salt = new byte[saltSize];
            using (var rng = RandomNumberGenerator.Create())
                rng.GetBytes(salt);

            byte[] hash = ComputeHash(password, salt, iterations, hashSize);
            return (hash, salt, iterations);
        }

        public static byte[] ComputeHash(string password, byte[] salt, int iterations, int hashSizeBytes)
        {
            if (password == null) throw new ArgumentNullException(nameof(password));
            if (salt == null || salt.Length == 0) throw new ArgumentException("Salt inválida.", nameof(salt));
            if (iterations <= 0) throw new ArgumentException("Iteraciones inválidas.", nameof(iterations));
            if (hashSizeBytes <= 0) throw new ArgumentException("Tamaño de hash inválido.", nameof(hashSizeBytes));

            using (var pbkdf2 = new Rfc2898DeriveBytes(password, salt, iterations))
            {
                return pbkdf2.GetBytes(hashSizeBytes);
            }
        }

        public static bool Verify(string password, byte[] storedHash, byte[] storedSalt, int iterations)
        {
            if (password == null) return false;
            if (storedHash == null || storedHash.Length == 0) return false;
            if (storedSalt == null || storedSalt.Length == 0) return false;
            if (iterations <= 0) return false;

            byte[] computed = ComputeHash(password, storedSalt, iterations, storedHash.Length);
            return FixedTimeEquals(storedHash, computed);
        }

        public static bool FixedTimeEquals(byte[] a, byte[] b)
        {
            if (a == null || b == null) return false;
            if (a.Length != b.Length) return false;

            int diff = 0;
            for (int i = 0; i < a.Length; i++)
                diff |= a[i] ^ b[i];

            return diff == 0;
        }
    }
}
