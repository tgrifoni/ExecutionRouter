namespace ExecutionRouter.Domain.ValueObjects;

/// <summary>
/// Represents the overall execution status
/// </summary>
public enum ExecutionStatus
{
    Success,
    Failed,
    Timeout,
    Cancelled
}