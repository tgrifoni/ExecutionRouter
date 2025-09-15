using ExecutionRouter.Domain.ValueObjects;

namespace ExecutionRouter.Domain.Entities;

/// <summary>
/// Represents a request for remote execution
/// </summary>
public sealed class ExecutionRequest
{
    public RequestId RequestId { get; private set; }
    public CorrelationId? CorrelationId { get; private set; }
    public ExecutorType ExecutorType { get; private set; }
    public string Method { get; private set; }
    public string Path { get; private set; }
    public Dictionary<string, string> QueryParameters { get; private set; }
    public Dictionary<string, string> Headers { get; private set; }
    public string? Body { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public TimeSpan TimeoutSeconds { get; private set; }

    private ExecutionRequest(
        RequestId requestId,
        CorrelationId? correlationId,
        ExecutorType executorType,
        string method,
        string path,
        TimeSpan timeoutSeconds,
        Dictionary<string, string> queryParameters,
        Dictionary<string, string> headers,
        string? body)
    {
        RequestId = requestId;
        CorrelationId = correlationId;
        ExecutorType = executorType;
        Method = method;
        Path = path;
        TimeoutSeconds = timeoutSeconds;
        QueryParameters = queryParameters;
        Headers = headers;
        Body = body;
        CreatedAt = DateTime.UtcNow;
    }

    public static ExecutionRequest Create(
        string? requestId,
        string? correlationId,
        string executorType,
        string method,
        string path,
        TimeSpan timeoutSeconds,
        Dictionary<string, string> queryParameters,
        Dictionary<string, string> headers,
        string? body)
    {
        var parsedRequestId = string.IsNullOrEmpty(requestId)
            ? RequestId.Generate()
            : RequestId.FromString(requestId);
        
        var parsedCorrelationId = string.IsNullOrEmpty(correlationId)
            ? null
            : CorrelationId.FromString(correlationId);
        
        var parsedExecutorType = ExecutorType.FromString(executorType);
        
        return new ExecutionRequest(
            parsedRequestId,
            parsedCorrelationId,
            parsedExecutorType,
            method,
            path,
            timeoutSeconds,
            queryParameters,
            headers,
            body);
    }
}