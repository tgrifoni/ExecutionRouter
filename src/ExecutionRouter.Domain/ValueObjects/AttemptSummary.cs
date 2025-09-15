namespace ExecutionRouter.Domain.ValueObjects;

/// <summary>
/// Represents the outcome of a single execution attempt
/// </summary>
public sealed record AttemptSummary(
    int AttemptNumber,
    DateTime StartTime,
    DateTime EndTime,
    AttemptOutcome Outcome,
    string? ErrorMessage = null,
    bool IsTransient = false)
{
    public TimeSpan Duration => EndTime - StartTime;
}