using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using StackExchange.Redis;
using Leaderboard.Metrics;
using LeaderBoard.Settings;
using Microsoft.Extensions.Options;

namespace Leaderboard.Filters;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public sealed class IdempotencyAttribute : Attribute, IAsyncActionFilter
{
	public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
	{
        var settings = context.HttpContext.RequestServices.GetService<IOptions<FilterSettings>>()?.Value;
        if (settings is null)
        {
            await next();
            return;
        }

        var headerName = settings.IdempotencyHeaderName;
        var ttl = TimeSpan.FromSeconds(settings.DefaultTtlSeconds);

		var http = context.HttpContext;
		if (!http.Request.Headers.TryGetValue(headerName, out var keyValues) || string.IsNullOrWhiteSpace(keyValues))
		{
			context.Result = new BadRequestObjectResult(new { error = $"Missing {headerName} header" });
			return;
		}
		var key = keyValues.ToString().Trim();

		var userId = http.User?.FindFirst("sub")?.Value ?? http.User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "anon";
		var redisKey = $"idem:lb:{userId}:{key}";

		var mux = http.RequestServices.GetService<IConnectionMultiplexer>();
		if (mux is null)
		{
			await next();
			return;
		}
		var db = mux.GetDatabase();
		
		var exists = await db.KeyExistsAsync(redisKey);
		if (exists)
		{
			var endpoint = GetEndpoint(context);
			AppMetrics.IdempotencyConflictsTotal.WithLabels(userId, endpoint).Inc();
			context.Result = new ConflictObjectResult(new { 
				success = false,
				code = "IDEMPOTENT_CONFLICT", 
				message = "Request already processed successfully" 
			});
			return;
		}

		var executedContext = await next();
		
		if (executedContext.Result is ObjectResult objectResult)
		{
			var statusCode = objectResult.StatusCode ?? 200;
			if (statusCode >= settings.SuccessStatusCodeMin && statusCode < settings.SuccessStatusCodeMax)
			{
				await db.StringSetAsync(redisKey, "success", ttl);
			}
		}
		else if (executedContext.Result is StatusCodeResult statusResult)
		{
			if (statusResult.StatusCode >= settings.SuccessStatusCodeMin && statusResult.StatusCode < settings.SuccessStatusCodeMax)
			{
				await db.StringSetAsync(redisKey, "success", ttl);
			}
		}
		else if (executedContext.Exception == null)
		{
			await db.StringSetAsync(redisKey, "success", ttl);
		}
	}

	private static string GetEndpoint(ActionExecutingContext context)
	{
		var controller = context.Controller.GetType().Name.Replace("Controller", "");
		var action = context.ActionDescriptor.RouteValues["action"] ?? "Unknown";
		return $"{controller}.{action}";
	}
}


