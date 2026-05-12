using System.Security.Cryptography;
using System.Text;

namespace TaskManagementSystem.Helpers
{
    public static class SecurityHelper
    {
        public static string HashPassword(string plainTextPassword)
        {
            var hashBytes = SHA256.HashData(Encoding.UTF8.GetBytes(plainTextPassword));
            return Convert.ToHexString(hashBytes);
        }

        public static bool VerifyPassword(string plainTextPassword, string storedHash)
            => string.Equals(HashPassword(plainTextPassword), storedHash, StringComparison.OrdinalIgnoreCase);
    }
}
