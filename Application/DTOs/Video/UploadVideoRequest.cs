using Domain.Videos;

namespace Application.DTOs.Video;

public class UploadVideoRequest
{
    public Stream VideoStream { get; set; } = default!;
    public string VideoFileName { get; set; } = default!;
    public Stream? ThumbnailStream { get; set; }
    public string? ThumbnailFileName { get; set; }
    public string Title { get; set; } = default!;
    public string? Description { get; set; }
    public VideoVisibility Visibility { get; set; } = VideoVisibility.Public;
}
