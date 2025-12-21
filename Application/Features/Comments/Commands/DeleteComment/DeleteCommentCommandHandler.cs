using Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Comments.Commands.DeleteComment;

public class DeleteCommentCommandHandler : IRequestHandler<DeleteCommentCommand, bool>
{
    private readonly IApplicationDbContext _context;

    public DeleteCommentCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<bool> Handle(DeleteCommentCommand request, CancellationToken cancellationToken)
    {
        var comment = await _context.Comments
            .FirstOrDefaultAsync(c => c.Id == request.CommentId && !c.IsDeleted, cancellationToken);

        if (comment == null || comment.UserId != request.UserId)
            return false;

        var video = await _context.Videos
            .FirstOrDefaultAsync(v => v.Id == comment.VideoId && !v.IsDeleted, cancellationToken);

        if (video != null)
        {
            video.ApplyCommentRemoved();
        }

        comment.SoftDelete();
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }
}

