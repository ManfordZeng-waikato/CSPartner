using Application.Interfaces.Repositories;
using Domain.Videos;
using Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories;

public class VideoRepository : IVideoRepository
{
    private readonly AppDbContext _context;

    public VideoRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<HighlightVideo>> GetVideosAsync(int page, int pageSize)
    {
        return await _context.Videos
            .Where(v => !v.IsDeleted)
            .Include(v => v.Likes)
            .OrderByDescending(v => v.CreatedAtUtc)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<HighlightVideo?> GetVideoByIdAsync(Guid videoId)
    {
        return await _context.Videos
            .Include(v => v.Likes)
            .Include(v => v.Comments)
            .FirstOrDefaultAsync(v => v.Id == videoId && !v.IsDeleted);
    }

    public async Task<HighlightVideo> AddAsync(HighlightVideo video)
    {
        await _context.Videos.AddAsync(video);
        return video;
    }

    public Task UpdateAsync(HighlightVideo video)
    {
        _context.Videos.Update(video);
        return Task.CompletedTask;
    }

    public async Task<bool> ExistsAsync(Guid videoId)
    {
        return await _context.Videos
            .AnyAsync(v => v.Id == videoId && !v.IsDeleted);
    }

    public async Task<VideoLike?> GetVideoLikeAsync(Guid videoId, Guid userId)
    {
        return await _context.VideoLikes
            .FirstOrDefaultAsync(l => l.VideoId == videoId && l.UserId == userId);
    }

    public async Task AddVideoLikeAsync(VideoLike like)
    {
        await _context.VideoLikes.AddAsync(like);
    }

    public Task RemoveVideoLikeAsync(VideoLike like)
    {
        _context.VideoLikes.Remove(like);
        return Task.CompletedTask;
    }
}
