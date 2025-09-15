namespace ExecutionRouter.Domain.Entities;

/// <summary>
/// Represents a snapshot of the current metrics
/// </summary>
public sealed record MetricsSnapshot(
    long TotalRequests,
    long SuccessfulRequests,
    long FailedRequests,
    long TransientFailures,
    long RetriedRequests,
    double AverageLatencyMs,
    double P95LatencyMs,
    DateTime SnapshotTime);