namespace Domain.Exceptions;

/// <summary>
/// Exception thrown when a user attempts to perform an operation they are not authorized to perform
/// </summary>
public class UnauthorizedOperationException : DomainException
{
    public UnauthorizedOperationException(string message) : base(message)
    {
    }

    public UnauthorizedOperationException(string resourceType, Guid resourceId) 
        : base($"User is not authorized to perform this operation on {resourceType} {resourceId}")
    {
    }
}

