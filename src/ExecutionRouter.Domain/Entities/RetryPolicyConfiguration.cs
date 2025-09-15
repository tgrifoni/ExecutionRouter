namespace ExecutionRouter.Domain.Entities;

/// <summary>
/// Configuration for retry policy
/// </summary>
public sealed record RetryPolicyConfiguration
{
    public int MaxAttempts { get; private init; } = 3;
    public int BaseDelayMs { get; private init; } = 1000;
    public TimeSpan MaxDelay { get; private init; } = TimeSpan.FromSeconds(30);
    public double BackoffMultiplier { get; private init; } = 2.0;
    public bool UseJitter { get; private init; } = true;

    public static RetryPolicyConfiguration Default => new();
    
    public static RetryPolicyConfiguration FromOptions(int maxAttempts, int baseDelayMs, int maxDelayMs, double backoffMultiplier, bool useJitter) =>
        new()
        {
            MaxAttempts = Math.Max(1, Math.Min(10, maxAttempts)),
            BaseDelayMs = Math.Max(100, Math.Min(10000, baseDelayMs)),
            MaxDelay = TimeSpan.FromMilliseconds(Math.Max(1000, Math.Min(300000, maxDelayMs))),
            BackoffMultiplier = Math.Max(1.0, Math.Min(5.0, backoffMultiplier)),
            UseJitter = useJitter
        };
}