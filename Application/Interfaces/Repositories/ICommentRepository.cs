using Domain.Comments;

namespace Application.Interfaces.Repositories;

public interface ICommentRepository
{
    Task<IEnumerable<Comment>> GetCommentsByVideoIdAsync(Guid videoId);
    Task<Comment?> GetCommentByIdAsync(Guid commentId);
    Task<Comment> AddAsync(Comment comment);
    Task UpdateAsync(Comment comment);
    Task<bool> ExistsAsync(Guid commentId);
}
