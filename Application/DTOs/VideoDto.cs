using Domain.Videos;

namespace Application.DTOs;

public class VideoDto
{
    public Guid VideoId { get; set; }
    public Guid UploaderUserId { get; set; }
    public string Title { get; set; } = default!;
    public string? Description { get; set; }
    public string VideoUrl { get; set; } = default!;
    public string? ThumbnailUrl { get; set; }
    public int LikeCount { get; set; }
    public int CommentCount { get; set; }
    public long ViewCount { get; set; }
    public VideoVisibility Visibility { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public DateTime? UpdatedAtUtc { get; set; }
    public bool HasLiked { get; set; }
}
