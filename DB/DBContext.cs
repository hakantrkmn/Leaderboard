using Leaderboard.Users.Models;
using Microsoft.EntityFrameworkCore;
using Leaderboard.LeaderBoard.Models;

namespace Leaderboard.DB;

public class DBContext : DbContext
{
    public DbSet<User> Users => Set<User>();
    public DbSet<LeaderboardEntry> Leaderboard => Set<LeaderboardEntry>();

    public DbSet<LeaderboardAroundRow> LeaderboardAroundRows => Set<LeaderboardAroundRow>();

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

        var lb = modelBuilder.Entity<LeaderboardEntry>();
		lb.HasKey(e => new { e.UserId, e.GameMode });  
		lb.Property(e => e.Score).IsRequired();
		lb.Property(e => e.UpdatedAtUtc).HasDefaultValueSql("NOW() AT TIME ZONE 'utc'");
		lb.Property(e => e.RegistrationDateUtc)
			.IsRequired()
			.HasDefaultValueSql("NOW() AT TIME ZONE 'utc'");
		lb.Property(e => e.PlayerLevel).HasDefaultValue(1);
		lb.Property(e => e.TrophyCount).HasDefaultValue(0);
		lb.Property(e => e.GameMode).IsRequired().HasDefaultValue(GameMode.Classic);
		lb.HasOne<User>()
			.WithMany()
			.HasForeignKey(e => e.UserId)
			.OnDelete(DeleteBehavior.Cascade);

		lb.HasIndex(e => new { e.GameMode, e.Score, e.RegistrationDateUtc, e.PlayerLevel, e.TrophyCount })
			.HasDatabaseName("IX_Leaderboard_GameMode_Ranking");
		lb.HasIndex(e => new { e.UserId, e.GameMode })
			.HasDatabaseName("IX_Leaderboard_User_GameMode");

        modelBuilder.Entity<LeaderboardAroundRow>().HasNoKey();

    }
}