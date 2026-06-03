using SecureQrMvc.Models;

namespace SecureQrMvc.Services;

public interface IUserSecurityService
{
    void SetPassword(AppUser user, string password);
    bool VerifyPassword(AppUser user, string password);
    string Sha256(string input);
}
