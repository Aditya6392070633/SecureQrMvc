using System.Security.Cryptography;
using System.Text;
using SecureQrMvc.Models;

namespace SecureQrMvc.Services;

public class UserSecurityService : IUserSecurityService
{
    private const int Iterations = 150_000;
    private const int SaltSize = 16;
    private const int KeySize = 32;

    public void SetPassword(AppUser user, string password)
    {
        user.PasswordSalt = RandomNumberGenerator.GetBytes(SaltSize);
        user.PasswordHash = Rfc2898DeriveBytes.Pbkdf2(password, user.PasswordSalt, Iterations, HashAlgorithmName.SHA256, KeySize);
    }

    public bool VerifyPassword(AppUser user, string password)
    {
        var hash = Rfc2898DeriveBytes.Pbkdf2(password, user.PasswordSalt, Iterations, HashAlgorithmName.SHA256, KeySize);
        return CryptographicOperations.FixedTimeEquals(hash, user.PasswordHash);
    }

    public string Sha256(string input)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(input));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }
}
