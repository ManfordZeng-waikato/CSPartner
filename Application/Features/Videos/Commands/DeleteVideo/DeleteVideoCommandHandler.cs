using Application.Common.Interfaces;
using Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Videos.Commands.DeleteVideo;

public class DeleteVideoCommandHandler : IRequestHandler<DeleteVideoCommand, bool>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public DeleteVideoCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<bool> Handle(DeleteVideoCommand request, CancellationToken cancellationToken)
    {
        if (!_currentUserService.UserId.HasValue)
            throw new UnauthorizedAccessException("User must be authenticated to delete a video");

        var video = await _context.Videos
            .FirstOrDefaultAsync(v => v.Id == request.VideoId && !v.IsDeleted, cancellationToken);

        if (video == null)
            throw new VideoNotFoundException(request.VideoId);

        if (video.UploaderUserId != _currentUserService.UserId.Value)
            throw new UnauthorizedOperationException("video", request.VideoId);

        video.SoftDelete();
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }
}

