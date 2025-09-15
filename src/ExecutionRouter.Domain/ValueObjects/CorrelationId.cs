namespace ExecutionRouter.Domain.ValueObjects;

/// <summary>
/// Represents a correlation identifier for tracing across multiple requests
/// </summary>
public sealed record CorrelationId
{
    public string Value { get; }

    private CorrelationId(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("CorrelationId cannot be null or empty", nameof(value));
        }
        
        Value = value;
    }

    public static CorrelationId FromString(string value) => new(value);
    
    public static CorrelationId Generate() => new(Guid.NewGuid().ToString("N"));
    
    public override string ToString() => Value;
    
    public static implicit operator string(CorrelationId correlationId) => correlationId.Value;
}