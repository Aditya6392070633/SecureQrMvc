using Microsoft.EntityFrameworkCore;
using SecureQrMvc.Models;

namespace SecureQrMvc.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
    public DbSet<AppUser> Users => Set<AppUser>();
    public DbSet<QrLoginSession> QrLoginSessions => Set<QrLoginSession>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AppUser>().HasIndex(x => x.Email).IsUnique();
        modelBuilder.Entity<QrLoginSession>().HasIndex(x => x.SessionId).IsUnique();
        modelBuilder.Entity<QrLoginSession>().HasIndex(x => x.TokenHash);
    }
}
