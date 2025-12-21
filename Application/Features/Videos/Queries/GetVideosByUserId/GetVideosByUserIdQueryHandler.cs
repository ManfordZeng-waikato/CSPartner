using Application.Common.Interfaces;
using Application.DTOs.Video;
using Application.Mappings;
using Domain.Videos;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Videos.Queries.GetVideosByUserId;

public class GetVideosByUserIdQueryHandler : IRequestHandler<GetVideosByUserIdQuery, IEnumerable<VideoDto>>
{
    private readonly IApplicationDbContext _context;

    public GetVideosByUserIdQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<VideoDto>> Handle(GetVideosByUserIdQuery request, CancellationToken cancellationToken)
    {
        var videos = await _context.Videos
            .Where(v => v.UploaderUserId == request.UserId && !v.IsDeleted)
            .Include(v => v.Likes)
            .OrderByDescending(v => v.CreatedAtUtc)
            .ToListAsync(cancellationToken);

        var isViewingOwnVideos = request.CurrentUserId.HasValue && request.CurrentUserId.Value == request.UserId;
        var videoDtos = new List<VideoDto>();

        foreach (var video in videos)
        {
            if (!isViewingOwnVideos && video.Visibility != VideoVisibility.Public)
            {
                continue;
            }

            bool hasLiked = false;
            if (request.CurrentUserId.HasValue)
            {
                hasLiked = await _context.VideoLikes
                    .AnyAsync(l => l.VideoId == video.VideoId && l.UserId == request.CurrentUserId.Value, cancellationToken);
            }

            videoDtos.Add(video.ToDto(hasLiked));
        }

        return videoDtos;
    }
}

