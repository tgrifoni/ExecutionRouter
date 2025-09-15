using ExecutionRouter.Domain.Constants;

namespace ExecutionRouter.Domain.ValueObjects;

/// <summary>
/// Represents the type of executor to use for the request
/// </summary>
public sealed record ExecutorType
{
    public string Value { get; }

    private ExecutorType(string value)
    {
        Value = value;
    }

    public static ExecutorType Http => new(ExecutorTypes.Http);
    public static ExecutorType PowerShell => new(ExecutorTypes.PowerShell);

    public static ExecutorType FromString(string value)
    {
        return value.ToLowerInvariant() switch
        {
            ExecutorTypes.Http => Http,
            ExecutorTypes.PowerShell => PowerShell,
            _ => throw new ArgumentException($"Unknown executor type: {value}", nameof(value))
        };
    }

    public override string ToString() => Value;
    
    public static implicit operator string(ExecutorType executorType) => executorType.Value;
}