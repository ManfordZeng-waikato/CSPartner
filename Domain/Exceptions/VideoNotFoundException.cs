namespace Domain.Exceptions;

public class VideoNotFoundException : DomainException
{
    public VideoNotFoundException(Guid videoId) 
        : base($"视频 {videoId} 不存在或已删除")
    {
    }
}
