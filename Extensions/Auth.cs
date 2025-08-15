using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace Leaderboard.Extensions;

public static class AuthExtensions
{
	public static IServiceCollection AddJwtAuth(this IServiceCollection services, IConfiguration configuration)
	{
		var secret = Environment.GetEnvironmentVariable("JWT_SECRET") ?? "dev-insecure-secret-change";
		var issuer = Environment.GetEnvironmentVariable("JWT_ISSUER") ?? "LeaderboardAPI";
		var audience = Environment.GetEnvironmentVariable("JWT_AUDIENCE") ?? "LeaderboardUsers";
		var accessTokenMinutes = int.TryParse(Environment.GetEnvironmentVariable("JWT_ACCESS_TOKEN_MINUTES"), out var minutes) ? minutes : 60;
		
		var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));

		services.AddAuthentication(options =>
		{
			options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
			options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
		}).AddJwtBearer(options =>
		{
			options.RequireHttpsMetadata = false;
			options.SaveToken = true;
			options.TokenValidationParameters = new TokenValidationParameters
			{
				ValidateIssuer = true,
				ValidateAudience = true,
				ValidateIssuerSigningKey = true,
				ValidIssuer = issuer,
				ValidAudience = audience,
				IssuerSigningKey = key,
				ClockSkew = TimeSpan.FromSeconds(30)
			};
		});

		services.AddAuthorization();
		return services;
	}
}


