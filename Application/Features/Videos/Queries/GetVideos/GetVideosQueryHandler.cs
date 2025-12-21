using Application.Common.Interfaces;
using Application.DTOs.Video;
using Application.Mappings;
using Domain.Videos;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Videos.Queries.GetVideos;

public class GetVideosQueryHandler : IRequestHandler<GetVideosQuery, IEnumerable<VideoDto>>
{
    private readonly IApplicationDbContext _context;

    public GetVideosQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<VideoDto>> Handle(GetVideosQuery request, CancellationToken cancellationToken)
    {
        var videos = await _context.Videos
            .Where(v => !v.IsDeleted)
            .Include(v => v.Likes)
            .OrderByDescending(v => v.CreatedAtUtc)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        var videoDtos = new List<VideoDto>();

        foreach (var video in videos)
        {
            var isOwner = request.CurrentUserId.HasValue && request.CurrentUserId.Value == video.UploaderUserId;
            if (video.Visibility != VideoVisibility.Public && !isOwner)
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

