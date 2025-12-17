using Application.DTOs.Video;
using Application.DTOs.Comment;
using Domain.Comments;
using Domain.Videos;

namespace Application.Mappings;

public static class VideoMappings
{
    public static VideoDto ToDto(this HighlightVideo video, bool hasLiked = false)
    {
        return new VideoDto
        {
            VideoId = video.VideoId,
            UploaderUserId = video.UploaderUserId,
            Title = video.Title,
            Description = video.Description,
            VideoUrl = video.VideoUrl,
            ThumbnailUrl = video.ThumbnailUrl,
            LikeCount = video.LikeCount,
            CommentCount = video.CommentCount,
            ViewCount = video.ViewCount,
            Visibility = video.Visibility,
            CreatedAtUtc = video.CreatedAtUtc,
            UpdatedAtUtc = video.UpdatedAtUtc,
            HasLiked = hasLiked
        };
    }

    public static CommentDto ToDto(this Comment comment)
    {
        return new CommentDto
        {
            CommentId = comment.CommentId,
            VideoId = comment.VideoId,
            UserId = comment.UserId,
            ParentCommentId = comment.ParentCommentId,
            Content = comment.Content,
            CreatedAtUtc = comment.CreatedAtUtc,
            UpdatedAtUtc = comment.UpdatedAtUtc,
            Replies = comment.Replies.Select(r => r.ToDto()).ToList()
        };
    }
}
