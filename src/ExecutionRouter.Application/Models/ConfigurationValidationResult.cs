namespace ExecutionRouter.Application.Models;

/// <summary>
/// Configuration validation result containing errors, if any
/// </summary>
public class ConfigurationValidationResult(IEnumerable<string> errors)
{
    public bool IsValid => !Errors.Any();
    public IEnumerable<string> Errors { get; } = errors;

    public string GetErrorSummary()
    {
        return IsValid ? "Valid" : string.Join("; ", Errors);
    }
}