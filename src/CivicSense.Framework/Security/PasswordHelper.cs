
using System;
using System.Security.Cryptography;

namespace CivicSense.Framework.Security
{
    public static class PasswordHelper
    {
        private const int Iterations = 600_000;
        private const int SaltSize = 16;
        private const int HashSize = 32;

        private const string Algorithm = "PBKDF2-SHA256";
        private const string Version = "v1";

        public static string HashPassword(string password)
        {
            if (string.IsNullOrEmpty(password))
            {
                throw new ArgumentException(
                    "Password cannot be empty.");
            }

            byte[] salt =
                RandomNumberGenerator.GetBytes(SaltSize);

            byte[] hash = Rfc2898DeriveBytes.Pbkdf2(
                password,
                salt,
                Iterations,
                HashAlgorithmName.SHA256,
                HashSize);

            return string.Join("$",
                Version,
                Algorithm,
                Iterations.ToString(),
                Convert.ToBase64String(salt),
                Convert.ToBase64String(hash));
        }

        public static bool VerifyPassword(
            string password,
            string storedHash)
        {
            if (string.IsNullOrEmpty(password) ||
                string.IsNullOrWhiteSpace(storedHash))
            {
                return false;
            }

            string[] parts = storedHash.Split('$');

            if (parts.Length != 5 ||
                parts[0] != Version ||
                parts[1] != Algorithm)
            {
                return false;
            }

            if (!int.TryParse(parts[2], out int iterations) ||
                iterations != Iterations)
            {
                return false;
            }

            try
            {
                byte[] salt =
                    Convert.FromBase64String(parts[3]);

                byte[] storedPasswordHash =
                    Convert.FromBase64String(parts[4]);

                if (salt.Length != SaltSize ||
                    storedPasswordHash.Length != HashSize)
                {
                    return false;
                }

                byte[] calculatedHash =
                    Rfc2898DeriveBytes.Pbkdf2(
                        password,
                        salt,
                        iterations,
                        HashAlgorithmName.SHA256,
                        HashSize);

                return CryptographicOperations.FixedTimeEquals(
                    storedPasswordHash,
                    calculatedHash);
            }
            catch (FormatException)
            {
                return false;
            }
        }
    }
}
