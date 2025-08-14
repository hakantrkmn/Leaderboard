using Leaderboard.Users.Interfaces;
using Leaderboard.Users.Repositories;
using Leaderboard.Users.Services;
using Leaderboard.Auth.Interfaces;
using Leaderboard.Auth.Services;

namespace Leaderboard.Extensions;

public static class Modules
{
    public static IServiceCollection AddUsersModule(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<IUserRepository, EfUserRepository>();
        services.AddScoped<UsersService>();
        return services;
    }

    public static IServiceCollection AddAuthModule(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<ITokenService, TokenService>();
        return services;
    }
}