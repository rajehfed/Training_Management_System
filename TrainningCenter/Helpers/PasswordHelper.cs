using System;
using System.Security.Cryptography;
using System.Text;

namespace TrainningCenter.Helpers
{
    public static class PasswordHelper
    {
        private const int SaltSize = 8;
        private const int HashSize = 32;
        private const int Iterations = 100_000;

        public static string CreateHash(string password)
        {
            if (string.IsNullOrEmpty(password))
            {
                throw new ArgumentNullException(nameof(password));
            }

            byte[] salt = new byte[SaltSize];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(salt);
            }

            // Generate hash using PBKDF2
            byte[] hash = PBKDF2(password, salt, Iterations, HashSize);

            // Convert to Base 64
            string saltB64 = Convert.ToBase64String(salt);
            string hashB64 = Convert.ToBase64String(hash);

            return $"{Iterations}.{saltB64}.{hashB64}";
        }
        public static bool VerifyHash(string password, string storedHash)
        {
            if (password == null) { throw new ArgumentNullException(nameof(password)); }
            if (string.IsNullOrWhiteSpace(storedHash)) return false;

            try
            {
                // Seperate of the pattren
                var parts = storedHash.Split('.');
                if (parts.Length != 3) return false;

                int iteration = int.Parse(parts[0]);
                byte[] salt = Convert.FromBase64String(parts[1]);
                byte[] hash = Convert.FromBase64String(parts[2]);

                // return the hash with the same parameters
                byte[] testHash = PBKDF2(password, salt, iteration, hash.Length);

                // Compare with safe
                return CryptographicEqauls(hash, testHash);
            }
            catch { return false; }
        }
        private static byte[] PBKDF2(string password, byte[] salt, int iteration, int length)
        {
            using (var pdkdf2 =
                new Rfc2898DeriveBytes(
                    password,
                    salt,
                    iteration,
                    HashAlgorithmName.SHA256)
            )
            {
                return pdkdf2.GetBytes(length);
            }
        }
        private static bool CryptographicEqauls(byte[] a, byte[] b)
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
