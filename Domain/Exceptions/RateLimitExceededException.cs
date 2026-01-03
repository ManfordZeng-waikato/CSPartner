namespace Domain.Exceptions;

/// <summary>
/// Exception thrown when a rate limit is exceeded
/// </summary>
public class RateLimitExceededException : DomainException
{
    public RateLimitExceededException(string message) : base(message)
    {
    }

    /// <summary>
    /// Creates an exception with a message indicating the rate limit was exceeded
    /// </summary>
    /// <param name="operation">The operation that was rate limited (e.g., "like a video")</param>
    /// <param name="retryAfterSeconds">The number of seconds to wait before retrying</param>
    public static RateLimitExceededException ForOperation(string operation, int retryAfterSeconds)
        => new($"Rate limit exceeded for {operation}. Please wait {retryAfterSeconds} seconds before trying again.");
}

