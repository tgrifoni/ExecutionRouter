using ExecutionRouter.Domain.Entities;

namespace ExecutionRouter.Domain.Interfaces;

/// <summary>
/// Defines the contract for metrics collection
/// </summary>
public interface IMetricsCollector
{
    /// <summary>
    /// Records a request metric
    /// </summary>
    void RecordRequest();

    /// <summary>
    /// Records a successful request
    /// </summary>
    void RecordSuccess();

    /// <summary>
    /// Records a failed request
    /// </summary>
    void RecordFailure(bool isTransient);

    /// <summary>
    /// Records request latency
    /// </summary>
    void RecordLatency(TimeSpan latency);

    /// <summary>
    /// Gets current metrics snapshot
    /// </summary>
    MetricsSnapshot GetSnapshot();
}