namespace Domain.Exceptions;

public class VideoNotFoundException : DomainException
{
    public VideoNotFoundException(Guid videoId) 
        : base($"Video {videoId} does not exist or has been deleted")
    {
    }
}
