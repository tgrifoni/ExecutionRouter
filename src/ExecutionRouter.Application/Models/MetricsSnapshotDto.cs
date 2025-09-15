using ExecutionRouter.Domain.Entities;
using ExecutionRouter.Domain.Interfaces;

namespace ExecutionRouter.Application.Models;

/// <summary>
/// DTO for metrics snapshot
/// </summary>
public sealed record MetricsSnapshotDto(long TotalRequests,
    long SuccessfulRequests,
    long FailedRequests,
    long TransientFailures,
    long RetriedRequests,
    double AverageLatencyMilliseconds,
    double P95LatencyMilliseconds,
    DateTime SnapshotTime,
    double SuccessRate)
{
    public static MetricsSnapshotDto FromDomain(MetricsSnapshot snapshot)
    {
        var successRate = snapshot.TotalRequests > 0 
            ? (double)snapshot.SuccessfulRequests / snapshot.TotalRequests * 100
            : 0.0;

        return new MetricsSnapshotDto
        (
            snapshot.TotalRequests,
            snapshot.SuccessfulRequests,
            snapshot.FailedRequests,
            snapshot.TransientFailures,
            snapshot.RetriedRequests,
            snapshot.AverageLatencyMs,
            snapshot.P95LatencyMs,
            snapshot.SnapshotTime,
            SuccessRate: Math.Round(successRate, 2)
        );
    }
}