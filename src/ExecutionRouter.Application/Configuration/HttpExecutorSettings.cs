using System.ComponentModel.DataAnnotations;

namespace ExecutionRouter.Application.Configuration;

/// <summary>
/// HTTP executor configuration
/// </summary>
public class HttpExecutorSettings
{
    /// <summary>
    /// Default timeout for HTTP requests in seconds
    /// </summary>
    [Range(1, 300)]
    public int DefaultTimeoutSeconds { get; init; } = 30;

    /// <summary>
    /// Maximum number of concurrent HTTP requests
    /// </summary>
    [Range(1, 1000)]
    public int MaxConcurrentRequests { get; init; } = 100;

    /// <summary>
    /// Allowed target URL patterns (regex patterns)
    /// </summary>
    [Required]
    public IEnumerable<string> AllowedTargetPatterns { get; init; } =
    [
        "^https?://.*"
    ];

    /// <summary>
    /// Blocked target URL patterns (regex patterns)
    /// </summary>
    public IEnumerable<string> BlockedTargetPatterns { get; init; } =
    [
        "^https?://localhost.*",
        @"^https?://127\.0\.0\.1.*",
        @"^https?://0\.0\.0\.0.*",
        @"^https?://.*\.local.*"
    ];

    /// <summary>
    /// Headers that should not be forwarded to the target
    /// </summary>
    public IEnumerable<string> FilteredHeaders { get; init; } =
    [
        "host",
        "connection",
        "content-length",
        "transfer-encoding",
        "upgrade",
        "x-request-id",
        "x-correlation-id",
        "x-executor-type",
        "x-request-timeout"
    ];
}