using Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Comments.Commands.DeleteComment;

public class DeleteCommentCommandHandler : IRequestHandler<DeleteCommentCommand, bool>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public DeleteCommentCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<bool> Handle(DeleteCommentCommand request, CancellationToken cancellationToken)
    {
        if (!_currentUserService.UserId.HasValue)
            return false;

        var comment = await _context.Comments
            .FirstOrDefaultAsync(c => c.Id == request.CommentId && !c.IsDeleted, cancellationToken);

        if (comment == null || comment.UserId != _currentUserService.UserId.Value)
            return false;

        // Find all child comments recursively (replies to this comment and replies to replies)
        var allChildComments = await FindAllChildCommentsAsync(comment.CommentId, cancellationToken);

        var video = await _context.Videos
            .FirstOrDefaultAsync(v => v.Id == comment.VideoId && !v.IsDeleted, cancellationToken);

        // Soft delete all child comments first
        foreach (var childComment in allChildComments)
        {
            childComment.SoftDelete();
            if (video != null)
            {
                video.ApplyCommentRemoved();
            }
        }

        // Soft delete the parent comment
        comment.SoftDelete();
        if (video != null)
        {
            video.ApplyCommentRemoved();
        }

        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }

    private async Task<List<Domain.Comments.Comment>> FindAllChildCommentsAsync(Guid parentCommentId, CancellationToken cancellationToken)
    {
        var result = new List<Domain.Comments.Comment>();
        var visited = new HashSet<Guid>();
        var queue = new Queue<Guid>();
        queue.Enqueue(parentCommentId);

        while (queue.Count > 0)
        {
            var currentId = queue.Dequeue();
            if (visited.Contains(currentId))
                continue;
            
            visited.Add(currentId);

            // Find all comments that reply to the current comment
            var replies = await _context.Comments
                .Where(c => c.ParentCommentId == currentId && !c.IsDeleted)
                .ToListAsync(cancellationToken);

            foreach (var reply in replies)
            {
                result.Add(reply);
                queue.Enqueue(reply.CommentId); // Also check for replies to this reply
            }
        }

        return result;
    }
}

