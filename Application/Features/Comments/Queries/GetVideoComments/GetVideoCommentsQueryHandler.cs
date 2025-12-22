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
        var comments = await _context.Comments
            .Where(c => c.VideoId == request.VideoId && !c.IsDeleted)
            .Include(c => c.Replies.Where(r => !r.IsDeleted).OrderByDescending(r => r.CreatedAtUtc))
            .OrderByDescending(c => c.CreatedAtUtc)
            .ToListAsync(cancellationToken);

        return comments.Select(c => c.ToDto());
    }
}

