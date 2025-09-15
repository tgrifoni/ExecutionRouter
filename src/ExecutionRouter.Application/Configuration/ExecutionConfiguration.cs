namespace ExecutionRouter.Application.Configuration;

/// <summary>
/// Main configuration settings for ExecutionRouter
/// </summary>
public class ExecutionConfiguration
{
    public const string SectionName = "ExecutionRouter";

    /// <summary>
    /// Security settings for request validation and limits
    /// </summary>
    public SecuritySettings Security { get; init; } = new();

    /// <summary>
    /// HTTP executor settings
    /// </summary>
    public HttpExecutorSettings HttpExecutor { get; init; } = new();

    /// <summary>
    /// PowerShell executor settings
    /// </summary>
    public PowerShellExecutorSettings PowerShellExecutor { get; init; } = new();

    /// <summary>
    /// Resilience policy settings
    /// </summary>
    public ResilienceSettings Resilience { get; init; } = new();

    /// <summary>
    /// Metrics and observability settings
    /// </summary>
    public ObservabilitySettings Observability { get; init; } = new();
}