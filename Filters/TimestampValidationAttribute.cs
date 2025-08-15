using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Leaderboard.Metrics;
using System.Security.Claims;

namespace Leaderboard.Filters;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public sealed class TimestampValidationAttribute : Attribute, IAsyncActionFilter
{
    private readonly TimeSpan _maxAge;
    private readonly TimeSpan _maxFuture;

    public TimestampValidationAttribute(int maxAgeMinutes = 10, int maxFutureMinutes = 2)
    {
        _maxAge = TimeSpan.FromMinutes(maxAgeMinutes);
        _maxFuture = TimeSpan.FromMinutes(maxFutureMinutes);
    }

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var http = context.HttpContext;
        var userId = GetUserId(http);
        var endpoint = GetEndpoint(context);
        
        // X-Timestamp header kontrolü
        if (!http.Request.Headers.TryGetValue("X-Timestamp", out var timestampHeader) || 
            string.IsNullOrWhiteSpace(timestampHeader))
        {
            AppMetrics.SecurityValidationFailuresTotal.WithLabels("missing_timestamp", userId, endpoint).Inc();
            context.Result = new BadRequestObjectResult(new { 
                success = false, 
                message = "Missing X-Timestamp header. Please include UTC timestamp." 
            });
            return;
        }

        // Timestamp parse kontrolü
        if (!long.TryParse(timestampHeader.ToString().Trim(), out var timestamp))
        {
            AppMetrics.SecurityValidationFailuresTotal.WithLabels("invalid_timestamp_format", userId, endpoint).Inc();
            context.Result = new BadRequestObjectResult(new { 
                success = false, 
                message = "Invalid timestamp format. Use Unix timestamp (seconds since epoch)." 
            });
            return;
        }

        var requestTime = DateTimeOffset.FromUnixTimeSeconds(timestamp);
        var now = DateTimeOffset.UtcNow;
        var age = now - requestTime;
        
        // Timestamp age'i record et (success/failure bağımsız)
        AppMetrics.TimestampAgeSeconds.WithLabels(endpoint, "measured").Observe(Math.Abs(age.TotalSeconds));
        
        // Çok eski request kontrolü (replay attack prevention)
        if (age > _maxAge)
        {
            AppMetrics.ReplayAttackAttemptsTotal.WithLabels(userId, "timestamp_too_old", endpoint).Inc();
            AppMetrics.TimestampAgeSeconds.WithLabels(endpoint, "rejected_old").Observe(age.TotalSeconds);
            context.Result = new BadRequestObjectResult(new { 
                success = false, 
                message = $"Request too old. Maximum age: {_maxAge.TotalMinutes} minutes. Current age: {age.TotalMinutes:F1} minutes." 
            });
            return;
        }
            
        // Gelecekten gelen request kontrolü (clock skew tolerance)
        if (requestTime > now + _maxFuture)
        {
            var futureAge = requestTime - now;
            AppMetrics.ReplayAttackAttemptsTotal.WithLabels(userId, "timestamp_future", endpoint).Inc();
            AppMetrics.TimestampAgeSeconds.WithLabels(endpoint, "rejected_future").Observe(-futureAge.TotalSeconds);
            context.Result = new BadRequestObjectResult(new { 
                success = false, 
                message = $"Request timestamp too far in future. Maximum: {_maxFuture.TotalMinutes} minutes. Difference: {futureAge.TotalMinutes:F1} minutes." 
            });
            return;
        }

        // Timestamp geçerli, action'ı çalıştır
        AppMetrics.TimestampAgeSeconds.WithLabels(endpoint, "accepted").Observe(age.TotalSeconds);
        await next();
    }

    private static string GetUserId(HttpContext http)
    {
        return http.User?.FindFirst("sub")?.Value ?? 
               http.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? 
               "anonymous";
    }

    private static string GetEndpoint(ActionExecutingContext context)
    {
        var controller = context.Controller.GetType().Name.Replace("Controller", "");
        var action = context.ActionDescriptor.RouteValues["action"] ?? "Unknown";
        return $"{controller}.{action}";
    }
}
