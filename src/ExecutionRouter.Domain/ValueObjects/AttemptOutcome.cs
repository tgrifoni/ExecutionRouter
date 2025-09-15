namespace ExecutionRouter.Domain.ValueObjects;

/// <summary>
/// Represents the outcome of a single attempt
/// </summary>
public enum AttemptOutcome
{
    Success,
    TransientFailure,
    PermanentFailure,
    Timeout,
    Cancelled
}