namespace Domain.Exceptions;

/// <summary>
/// Exception thrown when AI service quota is exceeded
/// </summary>
public class AiServiceQuotaExceededException : DomainException
{
    public AiServiceQuotaExceededException(string message) : base(message)
    {
    }

    public AiServiceQuotaExceededException(string message, Exception innerException) : base(message, innerException)
    {
    }

    /// <summary>
    /// Creates an exception for quota exceeded error
    /// </summary>
    public static AiServiceQuotaExceededException Create()
        => new("AI service quota has been exceeded. Please check your billing details or wait for quota reset.");
}

