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

    // Security Metrics
    public static readonly Counter ReplayAttackAttemptsTotal = Prometheus.Metrics.CreateCounter(
        name: "leaderboard_replay_attack_attempts_total",
        help: "Total replay attack attempts detected",
        configuration: new CounterConfiguration
        {
            LabelNames = new[] { "user_id", "reason", "endpoint" }
        }
    );

    public static readonly Counter IdempotencyConflictsTotal = Prometheus.Metrics.CreateCounter(
        name: "leaderboard_idempotency_conflicts_total",
        help: "Total idempotency conflicts detected",
        configuration: new CounterConfiguration
        {
            LabelNames = new[] { "user_id", "endpoint" }
        }
    );

    public static readonly Histogram TimestampAgeSeconds = Prometheus.Metrics.CreateHistogram(
        name: "leaderboard_request_timestamp_age_seconds",
        help: "Age of request timestamps in seconds",
        configuration: new HistogramConfiguration
        {
            LabelNames = new[] { "endpoint", "status" },
            Buckets = new[] { 0.0, 30.0, 60.0, 120.0, 300.0, 600.0, 1800.0, 3600.0 } // 0s to 1h
        }
    );

    public static readonly Counter SecurityValidationFailuresTotal = Prometheus.Metrics.CreateCounter(
        name: "leaderboard_security_validation_failures_total",
        help: "Total security validation failures",
        configuration: new CounterConfiguration
        {
            LabelNames = new[] { "validation_type", "user_id", "endpoint" }
        }
    );
}


