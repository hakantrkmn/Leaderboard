using System.Threading.RateLimiting;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.DependencyInjection;

namespace Leaderboard.Extensions;

public static class RateLimit
{
	public static IServiceCollection AddRateLimitingPolicies(this IServiceCollection services)
	{
		services.AddRateLimiter(options =>
		{
			options.AddPolicy("login", http => RateLimitPartition.GetFixedWindowLimiter(
				partitionKey: http.Connection.RemoteIpAddress?.ToString() ?? "anon",
				factory: _ => new FixedWindowRateLimiterOptions
				{
					PermitLimit = 5,
					Window = TimeSpan.FromMinutes(1),
					QueueLimit = 0
				}
			));

			options.AddPolicy("submit", http => RateLimitPartition.GetFixedWindowLimiter(
				partitionKey: http.User?.Identity?.Name ?? http.Connection.RemoteIpAddress?.ToString() ?? "anon",
				factory: _ => new FixedWindowRateLimiterOptions
				{
					PermitLimit = 30,
					Window = TimeSpan.FromMinutes(1),
					QueueLimit = 0
				}
			));
            options.AddPolicy("top", http => RateLimitPartition.GetFixedWindowLimiter(
				partitionKey: http.User?.Identity?.Name ?? http.Connection.RemoteIpAddress?.ToString() ?? "anon",
				factory: _ => new FixedWindowRateLimiterOptions
				{
					PermitLimit = 4,
					Window = TimeSpan.FromMinutes(1),
					QueueLimit = 0
				}
			));
            options.OnRejected = async (context, token) =>
            {
                context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
                context.HttpContext.Response.ContentType = "application/json";
                
                var response = new
                {
                    error = "Rate limit exceeded",
                    message = "Too many requests. Please try again later.",
                    retryAfter = context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter) 
                        ? retryAfter.TotalSeconds 
                        : 60,
                    limit = context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var limit) 
                        ? limit.ToString() 
                        : "unknown"
                };

                await context.HttpContext.Response.WriteAsJsonAsync(response, token);
            };
		});
		return services;
	}
}


