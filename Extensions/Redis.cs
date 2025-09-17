using StackExchange.Redis;

namespace Leaderboard.Extensions;

public static class Redis
{
    public static IServiceCollection AddRedis(this IServiceCollection services, IConfiguration configuration)
    {
		try
		{
			services.AddSingleton<IConnectionMultiplexer>(sp =>
			{
				var cs = GetRedisConnectionString(configuration);
				if (string.IsNullOrWhiteSpace(cs)) throw new InvalidOperationException("Redis connection string missing");

				Console.WriteLine($"🔌 Attempting to connect to Redis: {cs}");
				var multiplexer = ConnectionMultiplexer.Connect(cs);

				// Test the connection
				var db = multiplexer.GetDatabase();
				var pingResult = db.Ping();
				Console.WriteLine($"✅ Redis connection successful! Ping: {pingResult}");

				return multiplexer;
			});
		}
		catch (Exception ex)
		{
			// Redis bağlantısı başarısız olursa, null olarak kaydet
			// Bu sayede dependency injection çalışmaya devam eder
			services.AddSingleton<IConnectionMultiplexer>((IConnectionMultiplexer?)null);
			Console.WriteLine($"❌ Redis connection failed: {ex.Message}");
			Console.WriteLine($"🔍 Exception type: {ex.GetType().FullName}");
			if (ex.InnerException != null)
			{
				Console.WriteLine($"🔍 Inner exception: {ex.InnerException.Message}");
			}
			Console.WriteLine("ℹ️ Continuing without Redis services.");
		}
		return services;
    }

    private static string GetRedisConnectionString(IConfiguration configuration)
    {
        // First try environment variable
        var envCs = Environment.GetEnvironmentVariable("REDIS_CONNECTION_STRING");
        if (!string.IsNullOrEmpty(envCs))
        {
            Console.WriteLine("🔧 Using Redis from environment variable");
            return envCs;
        }

        // Check if we're in production environment
        var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
        if (environment == "Production")
        {
            var prodCs = configuration["Production:Redis"];
            if (!string.IsNullOrEmpty(prodCs))
            {
                Console.WriteLine("🏭 Using Redis from appsettings (production)");
                return prodCs;
            }
        }

        // Then try standard connection string
        var standardCs = configuration.GetConnectionString("Redis");
        if (!string.IsNullOrEmpty(standardCs))
        {
            var env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development";
            var message = env == "Production" ? "🏭 Using Redis from appsettings (production)" : "💻 Using Redis from appsettings (local)";
            Console.WriteLine(message);
            return standardCs;
        }

        // Finally try Docker-specific connection string
        var dockerCs = configuration["Docker:Redis"];
        if (!string.IsNullOrEmpty(dockerCs))
        {
            Console.WriteLine("🐳 Using Redis from appsettings (docker)");
            return dockerCs;
        }

        return null;
    }
}