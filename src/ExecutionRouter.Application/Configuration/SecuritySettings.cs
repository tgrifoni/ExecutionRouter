using System.ComponentModel.DataAnnotations;
using System.Net.Mime;
using ExecutionRouter.Domain.Constants;

namespace ExecutionRouter.Application.Configuration;

/// <summary>
/// Security and validation configuration
/// </summary>
public class SecuritySettings
{
    /// <summary>
    /// Maximum request body size in bytes (default: 10MB)
    /// </summary>
    [Range(1024, 100_000_000)]
    public long MaxRequestBodySizeBytes { get; init; } = 10_485_760;

    /// <summary>
    /// Maximum number of headers per request
    /// </summary>
    [Range(1, 100)]
    public int MaxHeaderCount { get; init; } = 50;

    /// <summary>
    /// Maximum header value length
    /// </summary>
    [Range(100, 10_000)]
    public int MaxHeaderValueLength { get; init; } = 2048;

    /// <summary>
    /// Maximum query parameter count
    /// </summary>
    [Range(1, 100)]
    public int MaxQueryParameterCount { get; init; } = 50;

    /// <summary>
    /// Maximum query parameter value length
    /// </summary>
    [Range(100, 10_000)]
    public int MaxQueryParameterValueLength { get; init; } = 2048;

    /// <summary>
    /// Maximum request timeout in seconds
    /// </summary>
    [Range(1, 300)]
    public int MaxTimeoutSeconds { get; init; } = 120;

    /// <summary>
    /// Headers that should be masked in logs for security
    /// </summary>
    [Required]
    public IEnumerable<string> SensitiveHeaders { get; init; } =
    [
        Headers.Standard.Authorization,
        Headers.Extended.XApiKey,
        Headers.Extended.XAuthToken,
        Headers.Standard.Cookie,
        Headers.Standard.SetCookie,
        Headers.Extended.XForwardedAuthorization,
        Headers.Standard.ProxyAuthorization
    ];

    /// <summary>
    /// Query parameter names that should be masked in logs
    /// </summary>
    public IEnumerable<string> SensitiveQueryParameters { get; init; } =
    [
        "password",
        "secret",
        "token",
        "key",
        "auth",
        "credential"
    ];

    /// <summary>
    /// Enable request body content validation
    /// </summary>
    public bool ValidateRequestBody { get; init; } = true;

    /// <summary>
    /// Allowed content types for request bodies
    /// </summary>
    public IEnumerable<string> AllowedContentTypes { get; init; } =
    [
        MediaTypeNames.Application.Json,
        MediaTypeNames.Application.Xml,
        MediaTypeNames.Text.Plain,
        MediaTypeNames.Text.Xml,
        MediaTypeNames.Application.FormUrlEncoded,
        MediaTypeNames.Multipart.FormData
    ];
}