using Application.Common.Interfaces;
using Domain.Videos;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Videos.Commands.ToggleLike;

public class ToggleLikeCommandHandler : IRequestHandler<ToggleLikeCommand, bool>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public ToggleLikeCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<bool> Handle(ToggleLikeCommand request, CancellationToken cancellationToken)
    {
        if (!_currentUserService.UserId.HasValue)
            return false;

        var video = await _context.Videos
            .FirstOrDefaultAsync(v => v.Id == request.VideoId && !v.IsDeleted, cancellationToken);

        if (video == null)
            return false;

        var existingLike = await _context.VideoLikes
            .FirstOrDefaultAsync(l => l.VideoId == request.VideoId && l.UserId == _currentUserService.UserId.Value, cancellationToken);

        if (existingLike != null)
        {
            _context.VideoLikes.Remove(existingLike);
            video.ApplyLikeRemoved();
        }
        else
        {
            var like = new VideoLike(request.VideoId, _currentUserService.UserId.Value);
            await _context.VideoLikes.AddAsync(like, cancellationToken);
            video.ApplyLikeAdded();
        }

        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }
}

