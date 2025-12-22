using Application.Common.Interfaces;
using Domain.Videos;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Collections.Concurrent;

namespace Application.Features.Videos.Commands.ToggleLike;

public class ToggleLikeCommandHandler : IRequestHandler<ToggleLikeCommand, bool>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    
    // In-memory cache for rate limiting: key = "userId_videoId", value = last request timestamp
    private static readonly ConcurrentDictionary<string, DateTime> _rateLimitCache = new();
    private static readonly TimeSpan _rateLimitWindow = TimeSpan.FromSeconds(2); // Allow one like per 2 seconds per user per video

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

        // Rate limiting: Check if user has recently liked/unliked this video
        var rateLimitKey = $"{_currentUserService.UserId.Value}_{request.VideoId}";
        if (_rateLimitCache.TryGetValue(rateLimitKey, out var lastRequestTime))
        {
            var timeSinceLastRequest = DateTime.UtcNow - lastRequestTime;
            if (timeSinceLastRequest < _rateLimitWindow)
            {
                // Rate limit exceeded - reject the request
                return false;
            }
        }

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
        
        // Update rate limit cache
        _rateLimitCache.AddOrUpdate(rateLimitKey, DateTime.UtcNow, (key, oldValue) => DateTime.UtcNow);
        
        // Clean up old entries periodically (older than 1 minute)
        if (_rateLimitCache.Count > 10000)
        {
            var cutoffTime = DateTime.UtcNow.AddMinutes(-1);
            var keysToRemove = _rateLimitCache
                .Where(kvp => kvp.Value < cutoffTime)
                .Select(kvp => kvp.Key)
                .ToList();
            foreach (var key in keysToRemove)
            {
                _rateLimitCache.TryRemove(key, out _);
            }
        }
        
        return true;
    }
}

