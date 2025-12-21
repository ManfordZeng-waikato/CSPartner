using Application.Common.Interfaces;
using Domain.Videos;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Videos.Commands.ToggleLike;

public class ToggleLikeCommandHandler : IRequestHandler<ToggleLikeCommand, bool>
{
    private readonly IApplicationDbContext _context;

    public ToggleLikeCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<bool> Handle(ToggleLikeCommand request, CancellationToken cancellationToken)
    {
        var video = await _context.Videos
            .FirstOrDefaultAsync(v => v.Id == request.VideoId && !v.IsDeleted, cancellationToken);

        if (video == null)
            return false;

        var existingLike = await _context.VideoLikes
            .FirstOrDefaultAsync(l => l.VideoId == request.VideoId && l.UserId == request.UserId, cancellationToken);

        if (existingLike != null)
        {
            _context.VideoLikes.Remove(existingLike);
            video.ApplyLikeRemoved();
        }
        else
        {
            var like = new VideoLike(request.VideoId, request.UserId);
            await _context.VideoLikes.AddAsync(like, cancellationToken);
            video.ApplyLikeAdded();
        }

        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }
}

