namespace ExecutionRouter.Domain.Exceptions;

/// <summary>
/// Exception thrown when execution fails permanently
/// </summary>
public sealed class ExecutionFailedException : DomainException
{
    public bool IsTransient { get; }

    public ExecutionFailedException(string message, bool isTransient = false) : base(message)
    {
        IsTransient = isTransient;
    }

    public ExecutionFailedException(string message, Exception innerException, bool isTransient = false) 
        : base(message, innerException)
    {
        IsTransient = isTransient;
    }
}