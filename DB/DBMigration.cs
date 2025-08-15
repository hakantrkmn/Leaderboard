

using Microsoft.EntityFrameworkCore;

namespace Leaderboard.DB;

public static class DBMigration
{
    public static async Task EnsureDatabaseMigrated(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<DBContext>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

        try
        {
            logger.LogInformation("🔍 Checking database migrations...");

            // Database connection test
            var canConnect = await context.Database.CanConnectAsync();
            if (!canConnect)
            {
                logger.LogError("❌ Cannot connect to database PLEASE CHECK THE ENV FILE");
                return;
            }

            logger.LogInformation("✅ Database connection successful");

            // Get migration status
            var appliedMigrations = await context.Database.GetAppliedMigrationsAsync();
            var pendingMigrations = await context.Database.GetPendingMigrationsAsync();

            logger.LogInformation("📊 Migration Status:");
            logger.LogInformation("   Applied: {Applied} migrations", appliedMigrations.Count());
            logger.LogInformation("   Pending: {Pending} migrations", pendingMigrations.Count());

            if (appliedMigrations.Any())
            {
                logger.LogInformation("   Latest applied: {Latest}", appliedMigrations.Last());
            }

            // Apply pending migrations
            if (pendingMigrations.Any())
            {
                logger.LogInformation("🔄 Applying {Count} pending migrations...", pendingMigrations.Count());
                foreach (var migration in pendingMigrations)
                {
                    logger.LogInformation("   - {Migration}", migration);
                }

                await context.Database.MigrateAsync();
                logger.LogInformation("✅ All migrations applied successfully");
            }
            else
            {
                logger.LogInformation("✅ Database is up to date, no migrations needed");
            }

            // Quick table validation
            var userCount = await context.Users.CountAsync();
            var leaderboardCount = await context.Leaderboard.CountAsync();
            logger.LogInformation("📊 Table Status: Users({Users}), Leaderboard({Leaderboard})", userCount, leaderboardCount);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "❌ Database migration failed: {Message}", ex.Message);
            logger.LogWarning("⚠️ Application will continue, but database may not be properly initialized");
            // Continue anyway - let the app start and health checks will detect issues
        }
    }
}

