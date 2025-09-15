namespace ExecutionRouter.Domain.Exceptions;

/// <summary>
/// Exception thrown when validation fails
/// </summary>
public sealed class ValidationException(string message, List<string> errors) : DomainException(message)
{
    public List<string> Errors { get; } = errors;

    public ValidationException(List<string> errors) : this("Validation failed", errors)
    {
    }
}