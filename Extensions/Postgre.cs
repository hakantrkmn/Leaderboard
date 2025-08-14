using Npgsql;
using Microsoft.EntityFrameworkCore;

namespace Leaderboard.Extensions;

public static class Postgre
{
    public static IServiceCollection AddPostgre(this IServiceCollection services, IConfiguration configuration)
    {
		services.AddDbContext<DbContext>(opt =>
		{
			var cs = configuration.GetValue<string>("POSTGRE_CONNECTION_STRING");
			if (string.IsNullOrWhiteSpace(cs)) throw new InvalidOperationException("POSTGRE_CONNECTION_STRING missing");
			opt.UseNpgsql(cs);
		});
		return services;
    }
}