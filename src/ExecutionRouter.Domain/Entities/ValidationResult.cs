namespace ExecutionRouter.Domain.Entities;

/// <summary>
/// Represents the result of validation
/// </summary>
public sealed record ValidationResult
{
    public bool IsValid { get; }
    public IEnumerable<string> Errors { get; }

    private ValidationResult(bool isValid, IEnumerable<string>? errors = null)
    {
        IsValid = isValid;
        Errors = errors ?? [];
    }

    public static ValidationResult Success() => new(true);
    
    public static ValidationResult Failure(params string[] errors) => new(false, errors);
}