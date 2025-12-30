using Application.Common.Interfaces;
using Domain.Exceptions;
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
            throw AuthenticationRequiredException.ForOperation("delete a comment");

        var comment = await _context.Comments
            .FirstOrDefaultAsync(c => c.Id == request.CommentId && !c.IsDeleted, cancellationToken);

        if (comment == null)
            throw new CommentNotFoundException(request.CommentId);

        if (comment.UserId != _currentUserService.UserId.Value)
            throw new UnauthorizedOperationException("comment", request.CommentId);

        // Query video first to check if it exists
        var video = await _context.Videos
            .FirstOrDefaultAsync(v => v.Id == comment.VideoId && !v.IsDeleted, cancellationToken);

        // Find all child comments recursively (replies to this comment and replies to replies)
        // Optimized: Single query to get all related comments, then build tree in memory
        var allChildComments = await FindAllChildCommentsAsync(comment.CommentId, comment.VideoId, cancellationToken);

        // Soft delete all child comments first
        foreach (var childComment in allChildComments)
        {
            childComment.SoftDelete();
        }

        // Soft delete the parent comment
        comment.SoftDelete();

        // Update video comment count once (1 parent + N child comments)
        if (video != null)
        {
            var totalCommentsToRemove = 1 + allChildComments.Count;
            for (int i = 0; i < totalCommentsToRemove; i++)
            {
                video.ApplyCommentRemoved();
            }
        }

        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }

    /// <summary>
    /// Find all child comments recursively using a single database query
    /// Optimized to avoid N+1 query problem
    /// </summary>
    private async Task<List<Domain.Comments.Comment>> FindAllChildCommentsAsync(Guid parentCommentId, Guid videoId, CancellationToken cancellationToken)
    {
        // Single query to get all potential child comments for this video
        // Build tree in memory to avoid N+1 queries
        var allComments = await _context.Comments
            .Where(c => !c.IsDeleted && c.ParentCommentId.HasValue && c.VideoId == videoId)
            .ToListAsync(cancellationToken);

        // Build a lookup for fast parent-child navigation
        var childrenLookup = allComments
            .GroupBy(c => c.ParentCommentId!.Value)
            .ToDictionary(g => g.Key, g => g.ToList());

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

            // Get children from lookup (O(1) operation)
            if (childrenLookup.TryGetValue(currentId, out var children))
            {
                foreach (var child in children)
                {
                    result.Add(child);
                    queue.Enqueue(child.CommentId); // Also check for replies to this reply
                }
            }
        }

        return result;
    }
}

