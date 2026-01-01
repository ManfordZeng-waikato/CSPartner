using Application.Common.Interfaces;
using Application.DTOs.Comment;
using Application.Mappings;
using Domain.Comments;
using Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Comments.Commands.CreateComment;

public class CreateCommentCommandHandler : IRequestHandler<CreateCommentCommand, CommentDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public CreateCommentCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<CommentDto> Handle(CreateCommentCommand request, CancellationToken cancellationToken)
    {
        if (!_currentUserService.UserId.HasValue)
            throw AuthenticationRequiredException.ForOperation("create a comment");

        var video = await _context.Videos
            .FirstOrDefaultAsync(v => v.Id == request.VideoId && !v.IsDeleted, cancellationToken);

        if (video == null)
            throw new VideoNotFoundException(request.VideoId);

        if (request.ParentCommentId.HasValue)
        {
            var parentComment = await _context.Comments
                .FirstOrDefaultAsync(c => c.Id == request.ParentCommentId.Value && !c.IsDeleted, cancellationToken);

            if (parentComment == null)
                throw new CommentNotFoundException(request.ParentCommentId.Value);
        }

        var comment = new Comment(request.VideoId, _currentUserService.UserId.Value, request.Content, request.ParentCommentId);
        await _context.Comments.AddAsync(comment, cancellationToken);
        video.ApplyCommentAdded();

        return comment.ToDto();
    }
}

