using Leaderboard.Users.Interfaces;
using Leaderboard.Users.Repositories;
using Leaderboard.Users.Services;

namespace Leaderboard.Extensions;

public static class Modules
{
    public static IServiceCollection AddUsersModule(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<IUserRepository, EfUserRepository>();
        services.AddScoped<UsersService>();
        return services;
    }
}