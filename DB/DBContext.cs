using Leaderboard.Users.Models;
using Microsoft.EntityFrameworkCore;

namespace Leaderboard.DB;

public class DBContext : DbContext
{
    public DbSet<User> Users => Set<User>();

    public DBContext(DbContextOptions<DBContext> options) : base(options){ }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var user = modelBuilder.Entity<User>(); 
        user.HasKey(u => u.Id);
        user.HasIndex(u => u.Username).IsUnique();
        user.Property(u => u.Username).HasMaxLength(50).IsRequired();
        user.Property(u => u.PasswordHash).IsRequired();
        user.Property(u => u.DeviceId).HasMaxLength(128).IsRequired();
        user.Property(u => u.RegistrationDate).HasDefaultValueSql("NOW() AT TIME ZONE 'utc'");
        user.Property(u => u.PlayerLevel).HasDefaultValue(1);
        user.Property(u => u.TrophyCount).HasDefaultValue(0);
    }
}