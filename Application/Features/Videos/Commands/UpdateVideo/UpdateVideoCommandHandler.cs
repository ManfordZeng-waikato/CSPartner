using Application.Common.Interfaces;
using Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Videos.Commands.UpdateVideo;

public class UpdateVideoCommandHandler : IRequestHandler<UpdateVideoCommand, bool>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public UpdateVideoCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<bool> Handle(UpdateVideoCommand request, CancellationToken cancellationToken)
    {
        if (!_currentUserService.UserId.HasValue)
            throw AuthenticationRequiredException.ForOperation("update a video");

        var video = await _context.Videos
            .FirstOrDefaultAsync(v => v.Id == request.VideoId && !v.IsDeleted, cancellationToken);

        if (video == null)
            throw new VideoNotFoundException(request.VideoId);

        if (video.UploaderUserId != _currentUserService.UserId.Value)
            throw new UnauthorizedOperationException("video", request.VideoId);

        if (request.Title != null)
            video.SetTitle(request.Title);

        if (request.Description != null)
            video.SetDescription(request.Description);

        if (request.ThumbnailUrl != null)
            video.SetThumbnailUrl(request.ThumbnailUrl);

        if (request.Visibility.HasValue)
            video.SetVisibility(request.Visibility.Value);

        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }
}

