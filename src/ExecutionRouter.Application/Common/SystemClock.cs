using ExecutionRouter.Domain.Interfaces;

namespace ExecutionRouter.Application.Common;

/// <summary>
/// System clock implementation for production use
/// </summary>
public sealed class SystemClock : ISystemClock
{
    public DateTime UtcNow => DateTime.UtcNow;
}