using Application.Common.Interfaces;
using Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Comments.Commands.UpdateComment;

public class UpdateCommentCommandHandler : IRequestHandler<UpdateCommentCommand, bool>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public UpdateCommentCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<bool> Handle(UpdateCommentCommand request, CancellationToken cancellationToken)
    {
        if (!_currentUserService.UserId.HasValue)
            throw new UnauthorizedAccessException("User must be authenticated to update a comment");

        var comment = await _context.Comments
            .FirstOrDefaultAsync(c => c.Id == request.CommentId && !c.IsDeleted, cancellationToken);

        if (comment == null)
            throw new CommentNotFoundException(request.CommentId);

        if (comment.UserId != _currentUserService.UserId.Value)
            throw new UnauthorizedOperationException("comment", request.CommentId);

        if (comment.IsDeleted)
            throw new InvalidCommentStateException($"Cannot update comment {request.CommentId} because it has been deleted");

        comment.SetContent(request.Content);
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }
}

