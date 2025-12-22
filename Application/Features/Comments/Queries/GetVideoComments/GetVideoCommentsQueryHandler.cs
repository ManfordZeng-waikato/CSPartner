using Application.Common.Interfaces;
using Application.DTOs.Comment;
using Application.Mappings;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Comments.Queries.GetVideoComments;

public class GetVideoCommentsQueryHandler : IRequestHandler<GetVideoCommentsQuery, IEnumerable<CommentDto>>
{
    private readonly IApplicationDbContext _context;

    public GetVideoCommentsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<CommentDto>> Handle(GetVideoCommentsQuery request, CancellationToken cancellationToken)
    {
        // Load all comments for this video (including all nested levels)
        var allComments = await _context.Comments
            .Where(c => c.VideoId == request.VideoId && !c.IsDeleted)
            .OrderByDescending(c => c.CreatedAtUtc)
            .ToListAsync(cancellationToken);

        // Build a dictionary for quick lookup
        var commentDict = allComments.ToDictionary(c => c.CommentId);

        // Build nested structure recursively
        var topLevelComments = allComments
            .Where(c => c.ParentCommentId == null)
            .OrderByDescending(c => c.CreatedAtUtc)
            .Select(c => BuildCommentTree(c, commentDict))
            .ToList();

        return topLevelComments;
    }

    private CommentDto BuildCommentTree(Domain.Comments.Comment comment, Dictionary<Guid, Domain.Comments.Comment> commentDict)
    {
        // Get parent comment's userId if this is a reply
        Guid? parentUserId = null;
        if (comment.ParentCommentId.HasValue && commentDict.TryGetValue(comment.ParentCommentId.Value, out var parentComment))
        {
            parentUserId = parentComment.UserId;
        }

        // Build DTO manually to avoid using ToDto() which relies on EF navigation properties
        var dto = new CommentDto
        {
            CommentId = comment.CommentId,
            VideoId = comment.VideoId,
            UserId = comment.UserId,
            ParentCommentId = comment.ParentCommentId,
            ParentUserId = parentUserId,
            Content = comment.Content,
            CreatedAtUtc = comment.CreatedAtUtc,
            UpdatedAtUtc = comment.UpdatedAtUtc,
            Replies = new List<CommentDto>()
        };
        
        // For root comments, find ALL replies (direct and indirect) that belong to this root comment
        // This includes replies to the root comment and replies to replies
        var allReplies = FindAllRepliesForRootComment(comment.CommentId, commentDict)
            .OrderByDescending(c => c.CreatedAtUtc)
            .Select(c => BuildReplyDto(c, commentDict))
            .ToList();

        dto.Replies = allReplies;
        return dto;
    }

    // Find all replies that belong to a root comment (including replies to replies)
    private List<Domain.Comments.Comment> FindAllRepliesForRootComment(Guid rootCommentId, Dictionary<Guid, Domain.Comments.Comment> commentDict)
    {
        var result = new List<Domain.Comments.Comment>();
        var visited = new HashSet<Guid>();
        var queue = new Queue<Guid>();
        queue.Enqueue(rootCommentId);

        while (queue.Count > 0)
        {
            var currentId = queue.Dequeue();
            if (visited.Contains(currentId))
                continue;
            
            visited.Add(currentId);

            // Find all comments that reply to the current comment
            var replies = commentDict.Values
                .Where(c => c.ParentCommentId == currentId)
                .ToList();

            foreach (var reply in replies)
            {
                result.Add(reply);
                queue.Enqueue(reply.CommentId); // Also check for replies to this reply
            }
        }

        return result;
    }

    // Build a reply DTO (non-recursive, since all replies are flat)
    private CommentDto BuildReplyDto(Domain.Comments.Comment comment, Dictionary<Guid, Domain.Comments.Comment> commentDict)
    {
        // Get parent comment's userId if this is a reply
        Guid? parentUserId = null;
        if (comment.ParentCommentId.HasValue && commentDict.TryGetValue(comment.ParentCommentId.Value, out var parentComment))
        {
            parentUserId = parentComment.UserId;
        }

        return new CommentDto
        {
            CommentId = comment.CommentId,
            VideoId = comment.VideoId,
            UserId = comment.UserId,
            ParentCommentId = comment.ParentCommentId,
            ParentUserId = parentUserId,
            Content = comment.Content,
            CreatedAtUtc = comment.CreatedAtUtc,
            UpdatedAtUtc = comment.UpdatedAtUtc,
            Replies = new List<CommentDto>() // All replies are flat, no nested replies
        };
    }
}

