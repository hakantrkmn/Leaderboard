using Prometheus;

namespace Leaderboard.Metrics;

public static class AppMetrics
{
    
    public static readonly Counter HttpRequestsTotal = Prometheus.Metrics.CreateCounter(
        name: "http_requests_total",
        help: "Total HTTP requests by method, path, and status",
        configuration: new CounterConfiguration 
        { 
            LabelNames = new[] { "method", "path", "status_code", "status_class" } 
        }
    );

    public static readonly Histogram HttpRequestDuration = Prometheus.Metrics.CreateHistogram(
        name: "http_request_duration_seconds",
        help: "HTTP request duration by method, path, and status",
        configuration: new HistogramConfiguration
        {
            LabelNames = new[] { "method", "path", "status_code", "status_class" },
            Buckets = Histogram.ExponentialBuckets(start: 0.001, factor: 2.0, count: 12)
        }
    );

    public static readonly Gauge HttpRequestsInProgress = Prometheus.Metrics.CreateGauge(
        name: "http_requests_in_progress",
        help: "Number of HTTP requests currently in progress",
        configuration: new GaugeConfiguration
        {
            LabelNames = new[] { "method", "path" }
        }
    );

    
    public static readonly Counter ApiRequestsTotal = Prometheus.Metrics.CreateCounter(
        name: "api_requests_total",
        help: "Total API requests by controller/action/status",
        configuration: new CounterConfiguration 
        { 
            LabelNames = new[] { "controller", "action", "status" } 
        }
    );

    public static readonly Histogram ApiRequestDuration = Prometheus.Metrics.CreateHistogram(
        name: "api_request_duration_seconds",
        help: "API request duration by controller/action/status",
        configuration: new HistogramConfiguration
        {
            LabelNames = new[] { "controller", "action", "status" },
            Buckets = Histogram.ExponentialBuckets(start: 0.01, factor: 2.0, count: 10)
        }
    );

    public static readonly Counter ScoreSubmissionsTotal = Prometheus.Metrics.CreateCounter(
        name: "leaderboard_score_submissions_total",
        help: "Total score submissions",
        configuration: new CounterConfiguration
        {
            LabelNames = new[] { "user_id", "score_range" }
        }
    );

    public static readonly Counter LoginAttemptsTotal = Prometheus.Metrics.CreateCounter(
        name: "leaderboard_login_attempts_total",
        help: "Total login attempts",
        configuration: new CounterConfiguration
        {
            LabelNames = new[] { "status" } 
        }
    );

    public static readonly Gauge ActiveUsers = Prometheus.Metrics.CreateGauge(
        name: "leaderboard_active_users",
        help: "Number of active users in the last hour"
    );
}


