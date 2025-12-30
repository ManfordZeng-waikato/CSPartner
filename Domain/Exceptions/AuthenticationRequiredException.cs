namespace Domain.Exceptions;

/// <summary>
/// Exception thrown when an operation requires authentication but the user is not authenticated
/// </summary>
public class AuthenticationRequiredException : DomainException
{
    public AuthenticationRequiredException(string message) : base(message)
    {
    }

    /// <summary>
    /// Creates an exception with a message indicating the operation that requires authentication
    /// </summary>
    /// <param name="operation">The operation that requires authentication (e.g., "delete a comment")</param>
    public static AuthenticationRequiredException ForOperation(string operation) 
        => new($"User must be authenticated to {operation}");
}
