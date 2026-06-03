using SecureQrMvc.Models;
using SecureQrMvc.Services;

namespace SecureQrMvc.Data;

public static class DbSeeder
{
    public static void Seed(AppDbContext db, IUserSecurityService security)
    {
        if (db.Users.Any()) return;
        var user = new AppUser
        {
            FullName = "Deepak Singh",
            Email = "admin@secureqr.local",
            CreatedAtUtc = DateTime.UtcNow
        };
        security.SetPassword(user, "Admin@12345");
        db.Users.Add(user);
        db.SaveChanges();
    }
}
