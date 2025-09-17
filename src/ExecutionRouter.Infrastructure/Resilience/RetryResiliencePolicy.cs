using ExecutionRouter.Domain.Entities;
using ExecutionRouter.Domain.Exceptions;
using ExecutionRouter.Domain.Interfaces;
using ExecutionRouter.Domain.ValueObjects;
using Polly;

namespace ExecutionRouter.Infrastructure.Resilience;

/// <summary>
/// Resilience policy implementation with retry and timeout handling
/// </summary>
public sealed class RetryResiliencePolicy(RetryPolicyConfiguration configuration, ISystemClock systemClock)
    : IResiliencePolicy
{
    public async Task<PolicyExecutionResult> ExecuteAsync(
        Func<CancellationToken, Task<ExecutorResult>> operation,
        PolicyExecutionContext context,
        CancellationToken cancellationToken = default)
    {
        var attemptSummaries = new List<AttemptSummary>();
        var attemptCount = 0;

        var retryPolicyWithCallbacks = Policy
            .Handle<ExecutionFailedException>(ex => ex.IsTransient)
            .Or<HttpRequestException>()
            .Or<TaskCanceledException>()
            .Or<TimeoutException>()
            .WaitAndRetryAsync(
                retryCount: configuration.MaxAttempts - 1,
                sleepDurationProvider: retryAttempt => 
                {
                    var delay = TimeSpan.FromMilliseconds(configuration.BaseDelayMilliseconds * Math.Pow(configuration.BackoffMultiplier, retryAttempt - 1));
                    if (delay > configuration.MaxDelayMilliseconds)
                    {
                        delay = configuration.MaxDelayMilliseconds;
                    }

                    if (!configuration.UseJitter)
                    {
                        return delay;
                    }
                    
                    var jitter = Random.Shared.NextDouble() * 0.2 - 0.1;
                    delay = TimeSpan.FromMilliseconds(delay.TotalMilliseconds * (1 + jitter));

                    return delay;
                },
                onRetry: (outcome, timespan, retryContext) =>
                {
                    var endTime = systemClock.UtcNow;
                    var startTime = retryContext.TryGetValue("StartTime", out var value)
                        ? (DateTime)value
                        : endTime.Subtract(timespan);
                    
                    var (attemptOutcome, isTransient) = ClassifyException(outcome);
                    
                    attemptSummaries.Add(new AttemptSummary(
                        attemptCount,
                        startTime,
                        endTime,
                        attemptOutcome,
                        outcome.Message,
                        isTransient));
                }
            );

        try
        {
            var result = await retryPolicyWithCallbacks.ExecuteAsync(async (retryContext, ct) =>
            {
                attemptCount++;
                var startTime = systemClock.UtcNow;
                retryContext["StartTime"] = startTime;
                
                var execResult = await operation(ct);
                var endTime = systemClock.UtcNow;
                
                attemptSummaries.Add(new AttemptSummary(
                    attemptCount,
                    startTime,
                    endTime,
                    AttemptOutcome.Success));
                    
                return execResult;
            }, new Context(), cancellationToken);

            return PolicyExecutionResult.Success(result, attemptSummaries);
        }
        catch (Exception ex)
        {
            var errorMessage = ex switch
            {
                OperationCanceledException when cancellationToken.IsCancellationRequested => "Operation was cancelled by user request",
                OperationCanceledException => $"Operation timed out after {context.Timeout}",
                ExecutionFailedException executionEx => executionEx.Message,
                _ => $"Operation failed after {configuration.MaxAttempts} attempts: {ex.Message}"
            };

            if (attemptSummaries.Count != 0 && attemptSummaries.Last().Outcome != AttemptOutcome.Success)
            {
                return PolicyExecutionResult.Failure(attemptSummaries, errorMessage);
            }
            
            var endTime = systemClock.UtcNow;
            var (outcome, isTransient) = ClassifyException(ex);
                
            attemptSummaries.Add(new AttemptSummary(
                attemptCount,
                endTime.AddMilliseconds(-100),
                endTime,
                outcome,
                ex.Message,
                isTransient));

            return PolicyExecutionResult.Failure(attemptSummaries, errorMessage);
        }
    }
    
    private static (AttemptOutcome outcome, bool isTransient) ClassifyException(Exception ex) =>
        ex switch
        {
            ExecutionTimeoutException => (AttemptOutcome.Timeout, true),
            ExecutionFailedException executionFailedException => executionFailedException.IsTransient 
                ? (AttemptOutcome.TransientFailure, true)
                : (AttemptOutcome.PermanentFailure, false),
            HttpRequestException httpRequestException => ClassifyHttpException(httpRequestException.Message.ToLowerInvariant()),
            TaskCanceledException => (AttemptOutcome.Timeout, true),
            OperationCanceledException => (AttemptOutcome.Cancelled, false),
            _ => (AttemptOutcome.PermanentFailure, false)
        };

    private static (AttemptOutcome outcome, bool isTransient) ClassifyHttpException(string message)
    {
        if (message.Contains("timeout") ||
            message.Contains("connection reset") ||
            message.Contains("network is unreachable") ||
            message.Contains("temporary failure") ||
            message.Contains("service unavailable") ||
            message.Contains("too many requests") ||
            message.Contains("502") ||
            message.Contains("503") ||
            message.Contains("504"))
        {
            return (AttemptOutcome.TransientFailure, true);
        }

        return (AttemptOutcome.PermanentFailure, false);
    }
}