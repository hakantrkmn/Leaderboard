using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using StackExchange.Redis;

namespace Leaderboard.Filters;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public sealed class IdempotencyAttribute : Attribute, IAsyncActionFilter
{
	private readonly string _headerName;
	private readonly TimeSpan _ttl;

	public IdempotencyAttribute(int ttlSeconds = 300, string headerName = "Idempotency-Key")
	{
		_headerName = headerName;
		_ttl = TimeSpan.FromSeconds(ttlSeconds);
	}

	public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
	{
		var http = context.HttpContext;
		if (!http.Request.Headers.TryGetValue(_headerName, out var keyValues) || string.IsNullOrWhiteSpace(keyValues))
		{
			context.Result = new BadRequestObjectResult(new { error = $"Missing {_headerName} header" });
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
			if (statusCode >= 200 && statusCode < 300)
			{
				await db.StringSetAsync(redisKey, "success", _ttl);
			}
		}
		else if (executedContext.Result is StatusCodeResult statusResult)
		{
			if (statusResult.StatusCode >= 200 && statusResult.StatusCode < 300)
			{
				await db.StringSetAsync(redisKey, "success", _ttl);
			}
		}
		else if (executedContext.Exception == null)
		{
			await db.StringSetAsync(redisKey, "success", _ttl);
		}
	}
}


