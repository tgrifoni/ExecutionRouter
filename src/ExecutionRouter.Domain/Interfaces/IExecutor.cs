using ExecutionRouter.Domain.Entities;
using ExecutionRouter.Domain.ValueObjects;

namespace ExecutionRouter.Domain.Interfaces;

/// <summary>
/// Defines the contract for executing requests
/// </summary>
public interface IExecutor
{
    /// <summary>
    /// Gets the executor type this implementation handles
    /// </summary>
    ExecutorType ExecutorType { get; }

    /// <summary>
    /// Executes the given request
    /// </summary>
    /// <param name="request">The execution request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The execution result</returns>
    Task<ExecutorResult> ExecuteAsync(ExecutionRequest request, CancellationToken cancellationToken = default);
}