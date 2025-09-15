using System.Collections.Concurrent;
using ExecutionRouter.Domain.Entities;
using ExecutionRouter.Domain.Interfaces;

namespace ExecutionRouter.Infrastructure.Observability;

/// <summary>
/// In-memory metrics collector implementation
/// </summary>
public sealed class InMemoryMetricsCollector(ISystemClock systemClock) : IMetricsCollector
{
    private readonly object _lock = new();
    
    // Counters
    private long _totalRequests;
    private long _successfulRequests;
    private long _failedRequests;
    private long _transientFailures;
    private long _retriedRequests;
    
    // Latency tracking
    private readonly ConcurrentQueue<double> _latencies = new();
    private const int MaxLatencyWindowSize = 1000;

    public void RecordRequest()
    {
        Interlocked.Increment(ref _totalRequests);
    }

    public void RecordSuccess()
    {
        Interlocked.Increment(ref _successfulRequests);
    }

    public void RecordFailure(bool isTransient)
    {
        Interlocked.Increment(ref _failedRequests);
        
        if (isTransient)
        {
            Interlocked.Increment(ref _transientFailures);
        }
    }

    public void RecordRetry()
    {
        Interlocked.Increment(ref _retriedRequests);
    }

    public void RecordLatency(TimeSpan latency)
    {
        var latencyMs = latency.TotalMilliseconds;
        _latencies.Enqueue(latencyMs);
        
        while (_latencies.Count > MaxLatencyWindowSize)
        {
            _latencies.TryDequeue(out _);
        }
    }

    public MetricsSnapshot GetSnapshot()
    {
        lock (_lock)
        {
            var latencyArray = _latencies.ToArray();
            
            var averageLatency = latencyArray.Length > 0 
                ? latencyArray.Average() 
                : 0.0;
            
            var p95Latency = CalculatePercentile95(latencyArray);
            
            return new MetricsSnapshot(
                _totalRequests,
                _successfulRequests,
                _failedRequests,
                _transientFailures,
                _retriedRequests,
                Math.Round(averageLatency, 2),
                Math.Round(p95Latency, 2),
                systemClock.UtcNow);
        }
    }

    private static double CalculatePercentile95(double[] values)
    {
        if (values.Length == 0)
        {
            return 0.0;
        }
        
        var sortedValues = values.OrderBy(x => x).ToArray();
        var index = (int)Math.Ceiling(0.95 * sortedValues.Length) - 1;
        index = Math.Max(0, Math.Min(sortedValues.Length - 1, index));
        
        return sortedValues[index];
    }
}