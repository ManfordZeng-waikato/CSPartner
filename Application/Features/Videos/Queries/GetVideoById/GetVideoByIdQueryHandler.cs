using Application.Common.Interfaces;
using Application.DTOs.Video;
using Application.Mappings;
using Domain.Videos;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Videos.Queries.GetVideoById;

public class GetVideoByIdQueryHandler : IRequestHandler<GetVideoByIdQuery, VideoDto?>
{
    private readonly IApplicationDbContext _context;

    public GetVideoByIdQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<VideoDto?> Handle(GetVideoByIdQuery request, CancellationToken cancellationToken)
    {
        var video = await _context.Videos
            .Include(v => v.Likes)
            .Include(v => v.Comments)
            .FirstOrDefaultAsync(v => v.Id == request.VideoId && !v.IsDeleted, cancellationToken);

        if (video == null)
            return null;

        var isOwner = request.CurrentUserId.HasValue && request.CurrentUserId.Value == video.UploaderUserId;
        if (video.Visibility != VideoVisibility.Public && !isOwner)
        {
            return null;
        }

        bool hasLiked = false;
        if (request.CurrentUserId.HasValue)
        {
            hasLiked = await _context.VideoLikes
                .AnyAsync(l => l.VideoId == video.VideoId && l.UserId == request.CurrentUserId.Value, cancellationToken);
        }

        return video.ToDto(hasLiked);
    }
}

