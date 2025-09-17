using Npgsql;
using Microsoft.EntityFrameworkCore;
using Leaderboard.DB;

namespace Leaderboard.Extensions;

public static class Postgre
{
    public static IServiceCollection AddPostgre(this IServiceCollection services, IConfiguration configuration)
    {
		services.AddDbContext<DBContext>(opt =>
		{
			var cs = GetPostgreConnectionString(configuration);
			if (string.IsNullOrWhiteSpace(cs)) throw new InvalidOperationException("PostgreSql connection string missing");
			opt.UseNpgsql(cs);
		});
		return services;
    }

    private static string GetPostgreConnectionString(IConfiguration configuration)
    {
        // First try environment variable
        var envCs = Environment.GetEnvironmentVariable("POSTGRE_CONNECTION_STRING");
        if (!string.IsNullOrEmpty(envCs))
        {
            Console.WriteLine("🔧 Using PostgreSQL from environment variable");
            return envCs;
        }

        // Check if we're in production environment
        var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
        if (environment == "Production")
        {
            var prodCs = configuration["Production:PostgreSql"];
            if (!string.IsNullOrEmpty(prodCs))
            {
                Console.WriteLine("🏭 Using PostgreSQL from appsettings (production)");
                return prodCs;
            }
        }

        // Then try standard connection string
        var standardCs = configuration.GetConnectionString("PostgreSql");
        if (!string.IsNullOrEmpty(standardCs))
        {
            var env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development";
            var message = env == "Production" ? "🏭 Using PostgreSQL from appsettings (production)" : "💻 Using PostgreSQL from appsettings (local)";
            Console.WriteLine(message);
            return standardCs;
        }

        // Finally try Docker-specific connection string
        var dockerCs = configuration["Docker:PostgreSql"];
        if (!string.IsNullOrEmpty(dockerCs))
        {
            Console.WriteLine("🐳 Using PostgreSQL from appsettings (docker)");
            return dockerCs;
        }

        return null;
    }
}