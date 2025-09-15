using ExecutionRouter.Domain.Entities;

namespace ExecutionRouter.Application.Models;

/// <summary>
/// Response DTO for execution
/// </summary>
public sealed record ExecutionResponseDto(string RequestId,
    string? CorrelationId,
    string ExecutorType,
    DateTime StartTime,
    DateTime EndTime,
    double DurationMilliseconds,
    string Status,
    IEnumerable<AttemptSummaryDto> AttemptSummaries,
    ExecutorResultDto Result,
    string? ErrorMessage)
{
    public static ExecutionResponseDto FromDomain(ExecutionResponse response) => new
        (
            response.RequestId.Value,
            response.CorrelationId?.Value,
            response.ExecutorType.Value,
            response.StartTime,
            response.EndTime,
            response.Duration.TotalMilliseconds,
            response.Status.ToString().ToLowerInvariant(),
            response.AttemptSummaries.Select(AttemptSummaryDto.FromDomain),
            ExecutorResultDto.FromDomain(response.Result),
            response.ErrorMessage
        );
}