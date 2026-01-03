namespace Domain.Exceptions;

/// <summary>
/// Exception thrown when an object key does not belong to the current user
/// </summary>
public class InvalidObjectKeyException : DomainException
{
    public InvalidObjectKeyException(string message) : base(message)
    {
    }

    public static InvalidObjectKeyException ForVideo(string objectKey)
        => new($"Video object key '{objectKey}' does not belong to the current user.");

    public static InvalidObjectKeyException ForThumbnail(string objectKey)
        => new($"Thumbnail object key '{objectKey}' does not belong to the current user.");
}

