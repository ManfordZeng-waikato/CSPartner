using Application.DTOs.Comment;

namespace Application.Interfaces.Services;

public interface ICommentService
{
    Task<IEnumerable<CommentDto>> GetVideoCommentsAsync(Guid videoId);
    Task<CommentDto> GetCommentByIdAsync(Guid commentId);
    Task<CommentDto> CreateCommentAsync(Guid videoId, Guid userId, string content, Guid? parentCommentId = null);
    Task<bool> UpdateCommentAsync(Guid commentId, Guid userId, string content);
    Task<bool> DeleteCommentAsync(Guid commentId, Guid userId);
}

