using StackExchange.Redis;

namespace Leaderboard.Extensions;

public static class Redis
{
    public static IServiceCollection AddRedis(this IServiceCollection services, IConfiguration configuration)
    {
		services.AddSingleton<IConnectionMultiplexer>(sp =>
		{
			var cs = configuration.GetValue<string>("REDIS_CONNECTION_STRING");
			if (string.IsNullOrWhiteSpace(cs)) throw new InvalidOperationException("REDIS_CONNECTION_STRING missing");
			return ConnectionMultiplexer.Connect(cs);
		});
		return services;
    }
}