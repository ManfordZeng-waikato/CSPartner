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

        // Fetch more videos than requested to account for visibility filtering
        // We fetch PageSize * 2 to ensure we have enough visible videos
        var fetchSize = request.PageSize * 2;
        var videos = await query
            .Include(v => v.Likes)
            .Take(fetchSize + 1) // +1 to check if there are more
            .ToListAsync(cancellationToken);

        // Check if there are more videos in the database (before filtering)
        var hasMoreInDb = videos.Count > fetchSize;
        if (hasMoreInDb)
        {
            videos = videos.Take(fetchSize).ToList();
        }

        // Filter by visibility and build DTOs
        var videoDtos = new List<VideoDto>();
        HighlightVideo? lastVisibleVideo = null;
        
        foreach (var video in videos)
        {
            var isOwner = request.CurrentUserId.HasValue && request.CurrentUserId.Value == video.UploaderUserId;
            if (video.Visibility != VideoVisibility.Public && !isOwner)
            {
                continue; // Skip private videos that user doesn't own
            }

            // Stop if we have enough visible videos
            if (videoDtos.Count >= request.PageSize)
            {
                break;
            }

            bool hasLiked = false;
            if (request.CurrentUserId.HasValue)
            {
                hasLiked = await _context.VideoLikes
                    .AnyAsync(l => l.VideoId == video.VideoId && l.UserId == request.CurrentUserId.Value, cancellationToken);
            }

            videoDtos.Add(video.ToDto(hasLiked));
            lastVisibleVideo = video; // Track last visible video for cursor
        }

        // Determine hasMore and nextCursor based on the last visible video (after filtering)
        // This ensures pagination works correctly even when private videos are filtered out
        var hasMore = false;
        string? nextCursor = null;
        
        if (lastVisibleVideo != null)
        {
            // Check if there are more videos after the last visible video
            var hasMoreAfterLastVisible = await _context.Videos
                .Where(v => !v.IsDeleted && 
                    (v.CreatedAtUtc < lastVisibleVideo.CreatedAtUtc || 
                     (v.CreatedAtUtc == lastVisibleVideo.CreatedAtUtc && v.Id.CompareTo(lastVisibleVideo.Id) < 0)))
                .AnyAsync(cancellationToken);
            
            if (hasMoreAfterLastVisible)
            {
                hasMore = true;
                nextCursor = CursorHelper.EncodeCursor(lastVisibleVideo.CreatedAtUtc, lastVisibleVideo.Id);
            }
        }
        // If we got exactly PageSize visible videos and there are more in DB, there might be more
        else if (videoDtos.Count == request.PageSize && hasMoreInDb)
        {
            // This case shouldn't happen if lastVisibleVideo is set, but handle it for safety
            hasMore = true;
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

