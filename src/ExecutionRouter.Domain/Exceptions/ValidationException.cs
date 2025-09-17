namespace ExecutionRouter.Domain.Exceptions;

/// <summary>
/// Exception thrown when validation fails
/// </summary>
public sealed class ValidationException(string message, IEnumerable<string> errors) : DomainException(message)
{
    public IEnumerable<string> Errors { get; } = errors;

    public ValidationException(IEnumerable<string> errors) : this("Validation failed", errors)
    {
    }
}