using Leaderboard.Users.Interfaces;
using Leaderboard.Users.Repositories;
using Leaderboard.Users.Services;
using Leaderboard.Auth.Interfaces;
using Leaderboard.Auth.Services;
using Leaderboard.LeaderBoard.Interfaces;
using Leaderboard.LeaderBoard.Repositories;
using Leaderboard.LeaderBoard.Services;

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

    public static IServiceCollection AddLeaderboardModule(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<ILeaderboardRepository, EfLeaderboardRepository>();
        services.AddScoped<ILeaderboardService, LeaderboardService>();
        
        return services;
    }

    public static IServiceCollection AddAllModules(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddUsersModule(configuration);
        services.AddAuthModule(configuration);
        services.AddLeaderboardModule(configuration);
        return services;
    }
}