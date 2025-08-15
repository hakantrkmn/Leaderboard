using Microsoft.AspNetCore.Mvc.Filters;
using Serilog.Context;
using Leaderboard.Metrics;

namespace Leaderboard.Filters;

public class LoggingActionFilter : IActionFilter
{
    private readonly ILogger<LoggingActionFilter> _logger;

    public LoggingActionFilter(ILogger<LoggingActionFilter> logger)
    {
        _logger = logger;
    }

    public void OnActionExecuting(ActionExecutingContext context)
    {
        var controllerName = context.Controller.GetType().Name;
        var actionName = context.ActionDescriptor.DisplayName;
        var userId = context.HttpContext.User?.FindFirst("sub")?.Value;

        using (LogContext.PushProperty("Controller", controllerName))
        using (LogContext.PushProperty("Action", actionName))
        using (LogContext.PushProperty("UserId", userId))
        {
            _logger.LogInformation("Executing {Controller}.{Action} for user {UserId}", 
                controllerName, actionName, userId);
        }
    }

    public void OnActionExecuted(ActionExecutedContext context)
    {
        var controllerName = context.Controller.GetType().Name;
        var actionName = context.ActionDescriptor.DisplayName;
        var statusCode = context.HttpContext.Response.StatusCode;
        var status = statusCode.ToString();

        AppMetrics.ApiRequestsTotal.WithLabels(controllerName, actionName, status).Inc();

        if (context.Exception != null)
        {
            _logger.LogError(context.Exception, 
                "Action {Controller}.{Action} failed with status {StatusCode}", 
                controllerName, actionName, statusCode);
        }
        else
        {
            _logger.LogInformation("Action {Controller}.{Action} completed with status {StatusCode}", 
                controllerName, actionName, statusCode);
        }
    }
}
