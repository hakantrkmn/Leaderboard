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
		var set = await db.StringSetAsync(redisKey, "1", _ttl, When.NotExists);
		if (!set)
		{
			context.Result = new ConflictObjectResult(new { code = "IDEMPOTENT_CONFLICT", message = "Duplicate request" });
			return;
		}

		await next();
	}
}


