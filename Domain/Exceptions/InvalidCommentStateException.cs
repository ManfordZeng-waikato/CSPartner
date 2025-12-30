namespace Domain.Exceptions;

/// <summary>
/// Exception thrown when a comment operation is attempted on a comment in an invalid state
/// </summary>
public class InvalidCommentStateException : DomainException
{
    public InvalidCommentStateException(string message) : base(message)
    {
    }
}

