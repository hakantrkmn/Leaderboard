using Microsoft.AspNetCore.Http;
using Serilog.Context;
using System.Diagnostics;
using Leaderboard.Metrics;

namespace Leaderboard.Middleware;

public class StructuredLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<StructuredLoggingMiddleware> _logger;

    public StructuredLoggingMiddleware(RequestDelegate next, ILogger<StructuredLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var correlationId = context.Request.Headers["X-Correlation-ID"].FirstOrDefault() 
            ?? Guid.NewGuid().ToString();
        
        context.Response.Headers["X-Correlation-ID"] = correlationId;

        var path = context.Request.Path.Value ?? "/";
        var method = context.Request.Method;
        
        AppMetrics.HttpRequestsInProgress.WithLabels(method, path).Inc();

        using (LogContext.PushProperty("CorrelationId", correlationId))
        using (LogContext.PushProperty("RequestPath", context.Request.Path))
        using (LogContext.PushProperty("RequestMethod", context.Request.Method))
        using (LogContext.PushProperty("UserAgent", context.Request.Headers["User-Agent"].FirstOrDefault()))
        using (LogContext.PushProperty("ClientIP", context.Connection.RemoteIpAddress?.ToString()))
        {
            var sw = Stopwatch.StartNew();
            
            try
            {
                await _next(context);
                sw.Stop();
                
                var statusCode = context.Response.StatusCode.ToString();
                var statusClass = (context.Response.StatusCode / 100).ToString() + "xx";
                var duration = sw.Elapsed.TotalSeconds;
                
                AppMetrics.HttpRequestsTotal.WithLabels(method, path, statusCode, statusClass).Inc();
                
                AppMetrics.HttpRequestDuration.WithLabels(method, path, statusCode, statusClass).Observe(duration);
                
                _logger.LogInformation(
                    "Request completed in {ElapsedMs}ms with status {StatusCode}",
                    sw.ElapsedMilliseconds,
                    context.Response.StatusCode);
            }
            catch (Exception ex)
            {
                sw.Stop();
                
                var statusCode = "500";
                var statusClass = "5xx";
                var duration = sw.Elapsed.TotalSeconds;
                
                AppMetrics.HttpRequestsTotal.WithLabels(method, path, statusCode, statusClass).Inc();
                AppMetrics.HttpRequestDuration.WithLabels(method, path, statusCode, statusClass).Observe(duration);
                
                _logger.LogError(ex, 
                    "Request failed after {ElapsedMs}ms",
                    sw.ElapsedMilliseconds);
                throw;
            }
            finally
            {
                AppMetrics.HttpRequestsInProgress.WithLabels(method, path).Dec();
            }
        }
    }
}
