using ExecutionRouter.Domain.ValueObjects;

namespace ExecutionRouter.Domain.Entities;

/// <summary>
/// Represents the result of a policy execution
/// </summary>
public sealed record PolicyExecutionResult(
    ExecutorResult Result,
    IEnumerable<AttemptSummary> AttemptSummaries,
    bool IsSuccess,
    string? ErrorMessage = null)
{
    public static PolicyExecutionResult Success(ExecutorResult result, IEnumerable<AttemptSummary> attemptSummaries)
        => new(result, attemptSummaries, true);

    public static PolicyExecutionResult Failure(IEnumerable<AttemptSummary> attemptSummaries, string errorMessage)
        => new(ExecutorResult.Empty, attemptSummaries, false, errorMessage);
}