using ExecutionRouter.Domain.Entities;

namespace ExecutionRouter.Application.Models;

/// <summary>
/// Request DTO for execution
/// </summary>
public sealed record ExecutionRequestDto(string? RequestId,
    string? CorrelationId,
    string ExecutorType,
    string Method,
    string Path,
    int TimeoutSeconds,
    Dictionary<string, string> QueryParameters,
    Dictionary<string, string> Headers,
    string? Body)
{
    public ExecutionRequest ToDomain()
    {
        return ExecutionRequest.Create(
            RequestId,
            CorrelationId,
            ExecutorType,
            Method,
            Path,
            timeoutSeconds: TimeSpan.FromSeconds(TimeoutSeconds),
            QueryParameters,
            Headers,
            Body);
    }
}