using Prometheus;

namespace Leaderboard.Extensions;


public static class Pipeline
{
    public static WebApplication UsePipeline(this WebApplication app)
    {
        app.UseSwagger();
        app.UseSwaggerUI();
		app.UseHttpsRedirection();
		app.UseMetricServer("/metrics");
		app.UseHttpMetrics();
		app.UseRateLimiter();
		app.UseAuthentication();
		app.UseAuthorization();
		app.MapControllers();
        return app;
    }
}