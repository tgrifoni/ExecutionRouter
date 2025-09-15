using ExecutionRouter.Domain.ValueObjects;

namespace ExecutionRouter.Application.Models;

/// <summary>
/// DTO for executor result
/// </summary>
public sealed record ExecutorResultDto(int? StatusCode,
    Dictionary<string, string> Headers,
    string? Body,
    Dictionary<string, object> Metadata)
{
    public static ExecutorResultDto FromDomain(ExecutorResult result) => new
        (
            result.StatusCode,
            Headers: new Dictionary<string, string>(result.Headers),
            result.Body,
            Metadata:new Dictionary<string, object>(result.Metadata)
        );
}