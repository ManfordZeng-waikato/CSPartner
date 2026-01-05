namespace Domain.Exceptions;

/// <summary>
/// Exception thrown when AI service encounters an error
/// </summary>
public class AiServiceException : DomainException
{
    public int StatusCode { get; }

    public AiServiceException(string message, int statusCode) : base(message)
    {
        StatusCode = statusCode;
    }

    public AiServiceException(string message, int statusCode, Exception innerException) : base(message, innerException)
    {
        StatusCode = statusCode;
    }

    /// <summary>
    /// Creates an exception for AI service errors
    /// </summary>
    public static AiServiceException Create(string message, int statusCode)
        => new(message, statusCode);
}

