using Prometheus;

namespace Leaderboard.Metrics;

public static class AppMetrics
{
	// Total API requests per action
    public static readonly Counter ApiRequestsTotal = Prometheus.Metrics.CreateCounter(
        name: "api_requests_total",
        help: "Total API requests by controller/action/status",
        configuration: new CounterConfiguration { LabelNames = new[] { "controller", "action", "status" } }
    );

	// Duration of API requests per action (seconds)
    public static readonly Histogram ApiRequestDuration = Prometheus.Metrics.CreateHistogram(
        name: "api_request_duration_seconds",
        help: "API request duration by controller/action/status",
        configuration: new HistogramConfiguration
        {
            LabelNames = new[] { "controller", "action", "status" },
            Buckets = Histogram.ExponentialBuckets(start: 0.01, factor: 2.0, count: 10)
        }
    );
}


