using System;
using System.Security.Claims;
using System.Security.Cryptography;

namespace EmployeeAccessSystem.Helpers
{
    public static class Helper
    {
        public static void CreatePasswordHash(string password, string secretKey, out byte[] hash, out byte[] salt)
        {
            salt = RandomNumberGenerator.GetBytes(16);

            string combinedPassword = password + secretKey;

            using var pbkdf2 = new Rfc2898DeriveBytes(
                combinedPassword,
                salt,
                100000,
                HashAlgorithmName.SHA256
            );

            hash = pbkdf2.GetBytes(32);
        }

        public static bool VerifyPassword(string password, string secretKey, byte[] storedHash, byte[] storedSalt)
        {
            string combinedPassword = password + secretKey;

            using var pbkdf2 = new Rfc2898DeriveBytes(
                combinedPassword,
                storedSalt,
                100000,
                HashAlgorithmName.SHA256
            );

            byte[] computedHash = pbkdf2.GetBytes(32);

            return CryptographicOperations.FixedTimeEquals(computedHash, storedHash);
        }

        public static string GetDisplayName(ClaimsPrincipal user)
        {
            if (user == null || user.Identity == null || !user.Identity.IsAuthenticated)
            {
                return "";
            }

            string displayName = user.FindFirst(ClaimTypes.Name)?.Value;

            if (string.IsNullOrWhiteSpace(displayName))
            {
                displayName = user.Identity.Name;
            }

            if (string.IsNullOrWhiteSpace(displayName))
            {
                displayName = user.FindFirst(ClaimTypes.Email)?.Value;
            }

            if (string.IsNullOrWhiteSpace(displayName))
            {
                displayName = "User";
            }

            return displayName;
        }
    }
}
