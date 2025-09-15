namespace ExecutionRouter.Domain.Exceptions;

/// <summary>
/// Base exception for the domain layer
/// </summary>
public abstract class DomainException : Exception
{
    protected DomainException(string message) : base(message) { }
    
    protected DomainException(string message, Exception innerException) : base(message, innerException) { }
}