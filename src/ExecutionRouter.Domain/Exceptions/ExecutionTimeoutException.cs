namespace ExecutionRouter.Domain.Exceptions;

/// <summary>
/// Exception thrown when execution times out
/// </summary>
public sealed class ExecutionTimeoutException(TimeSpan timeout)
    : DomainException($"Execution timed out after {timeout.TotalSeconds} seconds")
{
    public TimeSpan Timeout { get; } = timeout;
}