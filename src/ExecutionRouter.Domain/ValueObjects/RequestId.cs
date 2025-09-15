namespace ExecutionRouter.Domain.ValueObjects;

/// <summary>
/// Represents a unique request identifier
/// </summary>
public sealed record RequestId
{
    public string Value { get; }

    private RequestId(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("RequestId cannot be null or empty", nameof(value));
        }
        
        Value = value;
    }

    public static RequestId FromString(string value) => new(value);
    
    public static RequestId Generate() => new(Guid.NewGuid().ToString("N")[..12]);
    
    public override string ToString() => Value;
    
    public static implicit operator string(RequestId requestId) => requestId.Value;
}