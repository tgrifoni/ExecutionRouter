using ExecutionRouter.Domain.ValueObjects;

namespace ExecutionRouter.Domain.Entities;

/// <summary>
/// Provides context for policy execution
/// </summary>
public sealed record PolicyExecutionContext(RequestId RequestId, ExecutorType ExecutorType, TimeSpan Timeout);