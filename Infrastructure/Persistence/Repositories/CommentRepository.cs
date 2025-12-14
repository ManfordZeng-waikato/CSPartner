using Application.Interfaces.Repositories;
using Domain.Comments;
using Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories;

public class CommentRepository : ICommentRepository
{
    private readonly AppDbContext _context;

    public CommentRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Comment>> GetCommentsByVideoIdAsync(Guid videoId)
    {
        return await _context.Comments
            .Where(c => c.VideoId == videoId && !c.IsDeleted)
            .Include(c => c.Replies.Where(r => !r.IsDeleted))
            .OrderBy(c => c.CreatedAtUtc)
            .ToListAsync();
    }

    public async Task<Comment?> GetCommentByIdAsync(Guid commentId)
    {
        return await _context.Comments
            .FirstOrDefaultAsync(c => c.Id == commentId && !c.IsDeleted);
    }

    public async Task<Comment> AddAsync(Comment comment)
    {
        await _context.Comments.AddAsync(comment);
        return comment;
    }

    public Task UpdateAsync(Comment comment)
    {
        _context.Comments.Update(comment);
        return Task.CompletedTask;
    }

    public async Task<bool> ExistsAsync(Guid commentId)
    {
        return await _context.Comments
            .AnyAsync(c => c.Id == commentId && !c.IsDeleted);
    }
}
