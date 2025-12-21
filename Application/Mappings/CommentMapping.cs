using Application.DTOs.Comment;
using Domain.Comments;

namespace Application.Mappings;
public static class CommentMapping
{
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