using ExecutionRouter.Domain.ValueObjects;

namespace ExecutionRouter.Application.Models;

/// <summary>
/// DTO for attempt summary
/// </summary>
public sealed record AttemptSummaryDto(int AttemptNumber,
    DateTime StartTime,
    DateTime EndTime,
    double DurationMilliseconds,
    string Outcome,
    string? ErrorMessage,
    bool IsTransient)
{
    public static AttemptSummaryDto FromDomain(AttemptSummary attempt) => new
        (
            attempt.AttemptNumber,
            attempt.StartTime,
            attempt.EndTime,
            attempt.Duration.TotalMilliseconds,
            attempt.Outcome.ToString().ToLowerInvariant(),
            attempt.ErrorMessage,
            attempt.IsTransient
        );
}