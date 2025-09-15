namespace ExecutionRouter.Domain.Interfaces;

/// <summary>
/// Defines the contract for time abstraction (for testability)
/// </summary>
public interface ISystemClock
{
    /// <summary>
    /// Gets the current UTC date and time
    /// </summary>
    DateTime UtcNow { get; }
}