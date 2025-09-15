namespace ExecutionRouter.Domain.Entities;

/// <summary>
/// Configuration for retry policy
/// </summary>
public sealed record RetryPolicyConfiguration(
    int MaxAttempts,
    int BaseDelayMilliseconds,
    TimeSpan MaxDelayMilliseconds,
    double BackoffMultiplier,
    bool UseJitter)
{
    public static RetryPolicyConfiguration FromOptions(int maxAttempts,
        int baseDelayMilliseconds,
        int maxDelayMilliseconds,
        double backoffMultiplier,
        bool useJitter) =>
        new
        (
            maxAttempts,
            baseDelayMilliseconds,
            TimeSpan.FromMilliseconds(maxDelayMilliseconds),
            backoffMultiplier,
            useJitter
        );
}