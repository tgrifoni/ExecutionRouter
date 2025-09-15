using ExecutionRouter.Domain.Entities;
using ExecutionRouter.Domain.Exceptions;
using ExecutionRouter.Domain.Interfaces;
using ExecutionRouter.Domain.ValueObjects;

namespace ExecutionRouter.Infrastructure.Resilience;

/// <summary>
/// Resilience policy implementation with retry and timeout handling
/// </summary>
public sealed class RetryResiliencePolicy(RetryPolicyConfiguration configuration, ISystemClock systemClock)
    : IResiliencePolicy
{
    private readonly Random _random = new();

    public async Task<PolicyExecutionResult> ExecuteAsync(
        Func<CancellationToken, Task<ExecutorResult>> operation,
        PolicyExecutionContext context,
        CancellationToken cancellationToken = default)
    {
        var attemptSummaries = new List<AttemptSummary>();
        Exception? lastException = null;

        using var totalTimeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        totalTimeoutCts.CancelAfter(context.Timeout);

        for (var attempt = 1; attempt <= configuration.MaxAttempts; attempt++)
        {
            var attemptStartTime = systemClock.UtcNow;
            
            try
            {
                using var attemptTimeoutCts = CancellationTokenSource.CreateLinkedTokenSource(totalTimeoutCts.Token);
                var perAttemptTimeout = CalculatePerAttemptTimeout(context.Timeout, attempt);
                attemptTimeoutCts.CancelAfter(perAttemptTimeout);

                var result = await operation(attemptTimeoutCts.Token);
                
                var attemptEndTime = systemClock.UtcNow;
                attemptSummaries.Add(new AttemptSummary(
                    attempt,
                    attemptStartTime,
                    attemptEndTime,
                    AttemptOutcome.Success));

                return PolicyExecutionResult.Success(result, attemptSummaries);
            }
            catch (Exception ex)
            {
                var attemptEndTime = systemClock.UtcNow;
                lastException = ex;

                var (outcome, isTransient, attemptErrorMessage) = ClassifyException(ex);
                
                attemptSummaries.Add(new AttemptSummary(
                    attempt,
                    attemptStartTime,
                    attemptEndTime,
                    outcome,
                    attemptErrorMessage,
                    isTransient));

                if (!isTransient || attempt >= configuration.MaxAttempts)
                {
                    break;
                }

                if (attempt < configuration.MaxAttempts)
                {
                    var delay = CalculateDelay(attempt);
                    
                    try
                    {
                        await Task.Delay(delay, totalTimeoutCts.Token);
                    }
                    catch (OperationCanceledException)
                    {
                        break;
                    }
                }
            }
        }

        // All attempts failed
        var errorMessage = lastException?.Message ?? "Operation failed after all retry attempts";
        return PolicyExecutionResult.Failure(attemptSummaries, errorMessage);
    }

    private TimeSpan CalculatePerAttemptTimeout(TimeSpan totalTimeout, int attemptNumber)
    {
        var remainingAttempts = configuration.MaxAttempts - attemptNumber + 1;
        var perAttemptTimeout = TimeSpan.FromMilliseconds(totalTimeout.TotalMilliseconds / remainingAttempts);
        
        var minTimeout = TimeSpan.FromSeconds(5);
        return perAttemptTimeout < minTimeout ? minTimeout : perAttemptTimeout;
    }

    private (AttemptOutcome outcome, bool isTransient, string errorMessage) ClassifyException(Exception ex)
    {
        return ex switch
        {
            ExecutionTimeoutException => (AttemptOutcome.Timeout, true, ex.Message),
            ExecutionFailedException executionEx => executionEx.IsTransient 
                ? (AttemptOutcome.TransientFailure, true, ex.Message)
                : (AttemptOutcome.PermanentFailure, false, ex.Message),
            HttpRequestException httpEx => ClassifyHttpException(httpEx),
            TaskCanceledException => (AttemptOutcome.Timeout, true, "Request timed out"),
            OperationCanceledException => (AttemptOutcome.Cancelled, false, "Operation was cancelled"),
            _ => (AttemptOutcome.PermanentFailure, false, ex.Message)
        };
    }

    private static (AttemptOutcome outcome, bool isTransient, string errorMessage) ClassifyHttpException(HttpRequestException httpEx)
    {
        var message = httpEx.Message.ToLowerInvariant();
        
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
            return (AttemptOutcome.TransientFailure, true, httpEx.Message);
        }

        // Permanent errors
        return (AttemptOutcome.PermanentFailure, false, httpEx.Message);
    }

    private TimeSpan CalculateDelay(int attemptNumber)
    {
        // Exponential backoff: baseDelay * (backoffMultiplier ^ (attempt - 1))
        var delay = TimeSpan.FromMilliseconds(
            configuration.BaseDelayMs * Math.Pow(configuration.BackoffMultiplier, attemptNumber - 1));

        // Apply maximum delay cap
        if (delay > configuration.MaxDelay)
        {
            delay = configuration.MaxDelay;
        }

        // Apply jitter to avoid thundering herd
        if (configuration.UseJitter)
        {
            var jitterMs = (int)(delay.TotalMilliseconds * 0.1);
            var randomJitter = _random.Next(-jitterMs, jitterMs + 1);
            delay = delay.Add(TimeSpan.FromMilliseconds(randomJitter));
        }

        // Ensure minimum delay
        return delay < TimeSpan.Zero ? TimeSpan.Zero : delay;
    }
}