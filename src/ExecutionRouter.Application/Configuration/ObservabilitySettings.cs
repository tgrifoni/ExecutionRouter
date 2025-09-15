using System.ComponentModel.DataAnnotations;

namespace ExecutionRouter.Application.Configuration;

/// <summary>
/// Observability and monitoring configuration
/// </summary>
public class ObservabilitySettings
{
    /// <summary>
    /// Enable structured JSON logging
    /// </summary>
    public bool EnableStructuredLogging { get; init; } = true;

    /// <summary>
    /// Log level for the application
    /// </summary>
    [AllowedValues("Trace", "Debug", "Information", "Warning", "Error", "Critical",
        ErrorMessage = "LogLevel must be one of the following: Trace, Debug, Information, Warning, Error, Critical")]
    public string LogLevel { get; init; } = "Information";

    /// <summary>
    /// Enable metrics collection
    /// </summary>
    public bool EnableMetrics { get; init; } = true;

    /// <summary>
    /// Metrics retention period in hours
    /// </summary>
    [Range(1, 168)] // 1 hour to 1 week
    public int MetricsRetentionHours { get; init; } = 24;

    /// <summary>
    /// Enable detailed request/response logging
    /// </summary>
    public bool EnableDetailedLogging { get; init; } = true;

    /// <summary>
    /// Maximum log entry size in characters
    /// </summary>
    [Range(1000, 50000)]
    public int MaxLogEntrySizeChars { get; init; } = 10000;

    /// <summary>
    /// Enable performance monitoring
    /// </summary>
    public bool EnablePerformanceMonitoring { get; init; } = true;
}