using Application.Common;
using Application.Common.Interfaces;
using Application.DTOs.Common;
using Application.DTOs.Video;
using Application.Mappings;
using Domain.Videos;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Videos.Queries.GetVideos;

public class GetVideosQueryHandler : IRequestHandler<GetVideosQuery, CursorPagedResult<VideoDto>>
{
    private readonly IApplicationDbContext _context;

    public GetVideosQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<CursorPagedResult<VideoDto>> Handle(GetVideosQuery request, CancellationToken cancellationToken)
    {
        // Decode cursor if provided
        var cursor = CursorHelper.DecodeCursor(request.Cursor);
        
        // Build base query
        var query = _context.Videos
            .Where(v => !v.IsDeleted)
            .AsQueryable();

        // Apply cursor filter if provided
        // For DESC ordering: records after cursor have CreatedAt < cursorCreatedAt OR (CreatedAt == cursorCreatedAt AND Id < cursorId)
        if (cursor.HasValue)
        {
            var (cursorCreatedAt, cursorId) = cursor.Value;
            query = query.Where(v => 
                v.CreatedAtUtc < cursorCreatedAt || 
                (v.CreatedAtUtc == cursorCreatedAt && v.Id.CompareTo(cursorId) < 0));
        }

        // Order by CreatedAt DESC, then Id DESC (for consistent ordering)
        query = query
            .OrderByDescending(v => v.CreatedAtUtc)
            .ThenByDescending(v => v.Id);

        // Fetch one extra item to determine if there are more results
        var videos = await query
            .Include(v => v.Likes)
            .Take(request.PageSize + 1)
            .ToListAsync(cancellationToken);

        // Determine if there are more results
        var hasMore = videos.Count > request.PageSize;
        if (hasMore)
        {
            videos = videos.Take(request.PageSize).ToList();
        }

        // Filter by visibility and build DTOs
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

        // Generate next cursor from the last item in the filtered list
        // Use the last video from the original query (before visibility filtering)
        // to maintain consistent cursor positioning
        string? nextCursor = null;
        if (hasMore && videos.Count > 0)
        {
            var lastVideo = videos.Last();
            nextCursor = CursorHelper.EncodeCursor(lastVideo.CreatedAtUtc, lastVideo.Id);
        }

        return new CursorPagedResult<VideoDto>
        {
            Items = videoDtos,
            NextCursor = nextCursor,
            HasMore = hasMore,
            Count = videoDtos.Count
        };
    }
}

