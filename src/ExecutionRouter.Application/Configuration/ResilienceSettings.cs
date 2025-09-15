using System.ComponentModel.DataAnnotations;
using System.Net;

namespace ExecutionRouter.Application.Configuration;

/// <summary>
/// Resilience and retry configuration
/// </summary>
public class ResilienceSettings
{
    public const string SectionName = "ExecutionRouter:Resilience";
    
    /// <summary>
    /// Maximum number of retry attempts (0 to 5)
    /// </summary>
    [Range(0, 5)]
    public int MaxRetryAttempts { get; init; } = 3;

    /// <summary>
    /// Base delay for exponential backoff in milliseconds (100 to 10_000)
    /// </summary>
    [Range(100, 10000)]
    public int BaseDelayMilliseconds { get; init; } = 1000;
    
    /// <summary>
    /// Backoff multiplier for exponential backoff (1.0 to 5.0)
    /// </summary>
    [Range(1.0, 5.0)]
    public double BackoffMultiplier { get; init; } = 2.0;

    /// <summary>
    /// Maximum delay between retries in milliseconds (1000 to 60000)
    /// </summary>
    [Range(1000, 60000)]
    public int MaxDelayMilliseconds { get; init; } = 30000;

    /// <summary>
    /// If Jitter should be used for exponential backoff
    /// </summary>
    [Required]
    public bool UseJitter { get; init; } = true;

    /// <summary>
    /// HTTP status codes that should trigger retries
    /// </summary>
    [Required]
    public HashSet<int> RetriableHttpStatusCodes { get; init; } = 
    [
        (int)HttpStatusCode.RequestTimeout,
        (int)HttpStatusCode.TooManyRequests,
        (int)HttpStatusCode.InternalServerError,
        (int)HttpStatusCode.BadGateway,
        (int)HttpStatusCode.ServiceUnavailable,
        (int)HttpStatusCode.GatewayTimeout
    ];

    /// <summary>
    /// Exception types that should trigger retries
    /// </summary>
    [Required]
    public HashSet<string> RetriableExceptionTypes { get; init; } = 
    [
        "HttpRequestException",
        "TaskCanceledException", 
        "SocketException",
        "TimeoutException"
    ];
}