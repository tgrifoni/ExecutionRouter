using ExecutionRouter.Domain.Entities;
using ExecutionRouter.Domain.ValueObjects;

namespace ExecutionRouter.Domain.Interfaces;

/// <summary>
/// Defines the contract for resilience policies
/// </summary>
public interface IResiliencePolicy
{
    /// <summary>
    /// Executes the given function with resilience policies applied
    /// </summary>
    /// <param name="operation">The operation to execute</param>
    /// <param name="context">Execution context</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The policy execution result</returns>
    Task<PolicyExecutionResult> ExecuteAsync(
        Func<CancellationToken, Task<ExecutorResult>> operation,
        PolicyExecutionContext context,
        CancellationToken cancellationToken = default);
}