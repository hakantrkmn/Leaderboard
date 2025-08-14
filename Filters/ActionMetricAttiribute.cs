

using System.Diagnostics;
using Leaderboard.Metrics;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Leaderboard.Filters;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public sealed class ActionMetricAttribute : Attribute, IAsyncActionFilter
{
	public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
	{
		var route = context.HttpContext.GetEndpoint()?.Metadata.GetMetadata<RouteNameMetadata>();
		var controller = context.HttpContext.GetRouteValue("controller")?.ToString() ?? "unknown";
		var action = context.HttpContext.GetRouteValue("action")?.ToString() ?? route?.RouteName ?? context.HttpContext.Request.Path.ToString();

        var sw = Stopwatch.StartNew();
        try {
            await next();
            var statusCode = context.HttpContext.Response.StatusCode;
            AppMetrics.ApiRequestsTotal.WithLabels(controller, action, statusCode.ToString()).Inc();
            AppMetrics.ApiRequestDuration.WithLabels(controller, action, statusCode.ToString()).Observe(sw.Elapsed.TotalSeconds);
        }
        catch 
        {
            var statusCode = context.HttpContext.Response.StatusCode;
            AppMetrics.ApiRequestsTotal.WithLabels(controller, action, statusCode.ToString()).Inc();
            AppMetrics.ApiRequestDuration.WithLabels(controller, action, statusCode.ToString()).Observe(sw.Elapsed.TotalSeconds);
            throw;
        }
	}
}