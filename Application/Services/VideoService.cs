using Domain.Comments;
using Domain.Videos;
using Microsoft.EntityFrameworkCore;
using Persistence.Context;

namespace Application.Services;

public class VideoService : IVideoService
{
    private readonly AppDbContext _context;

    public VideoService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<HighlightVideo>> GetVideosAsync(int page = 1, int pageSize = 20)
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

    public async Task<HighlightVideo> CreateVideoAsync(Guid uploaderUserId, string title, string videoUrl, string? description = null, string? thumbnailUrl = null, VideoVisibility visibility = VideoVisibility.Public)
    {
        var video = new HighlightVideo(uploaderUserId, title, videoUrl, description, thumbnailUrl);
        if (visibility != VideoVisibility.Public)
        {
            video.SetVisibility(visibility);
        }
        _context.Videos.Add(video);
        await _context.SaveChangesAsync();
        return video;
    }

    public async Task<bool> UpdateVideoAsync(Guid videoId, Guid userId, string? title = null, string? description = null, string? thumbnailUrl = null, VideoVisibility? visibility = null)
    {
        var video = await _context.Videos.FirstOrDefaultAsync(v => v.Id == videoId && !v.IsDeleted);
        if (video == null || video.UploaderUserId != userId)
            return false;

        if (title != null)
            video.SetTitle(title);

        if (description != null)
            video.SetDescription(description);

        if (thumbnailUrl != null)
            video.SetThumbnailUrl(thumbnailUrl);

        if (visibility.HasValue)
            video.SetVisibility(visibility.Value);

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteVideoAsync(Guid videoId, Guid userId)
    {
        var video = await _context.Videos.FirstOrDefaultAsync(v => v.Id == videoId && !v.IsDeleted);
        if (video == null || video.UploaderUserId != userId)
            return false;

        video.SoftDelete();
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ToggleLikeAsync(Guid videoId, Guid userId)
    {
        var video = await _context.Videos
            .Include(v => v.Likes)
            .FirstOrDefaultAsync(v => v.Id == videoId && !v.IsDeleted);

        if (video == null)
            return false;

        var existingLike = await _context.VideoLikes
            .FirstOrDefaultAsync(l => l.VideoId == videoId && l.UserId == userId);

        if (existingLike != null)
        {
            _context.VideoLikes.Remove(existingLike);
            video.ApplyLikeRemoved();
        }
        else
        {
            var like = new VideoLike(videoId, userId);
            _context.VideoLikes.Add(like);
            video.ApplyLikeAdded();
        }

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> HasUserLikedAsync(Guid videoId, Guid userId)
    {
        return await _context.VideoLikes
            .AnyAsync(l => l.VideoId == videoId && l.UserId == userId);
    }

    public async Task IncreaseViewCountAsync(Guid videoId)
    {
        var video = await _context.Videos.FirstOrDefaultAsync(v => v.Id == videoId && !v.IsDeleted);
        if (video != null)
        {
            video.IncreaseView();
            await _context.SaveChangesAsync();
        }
    }

    public async Task<IEnumerable<Comment>> GetVideoCommentsAsync(Guid videoId)
    {
        return await _context.Comments
            .Where(c => c.VideoId == videoId && !c.IsDeleted)
            .Include(c => c.Replies.Where(r => !r.IsDeleted))
            .OrderBy(c => c.CreatedAtUtc)
            .ToListAsync();
    }

    public async Task<Comment> CreateCommentAsync(Guid videoId, Guid userId, string content, Guid? parentCommentId = null)
    {
        var video = await _context.Videos.FirstOrDefaultAsync(v => v.Id == videoId && !v.IsDeleted);
        if (video == null)
            throw new InvalidOperationException("视频不存在或已删除");

        if (parentCommentId.HasValue)
        {
            var parentComment = await _context.Comments.FirstOrDefaultAsync(c => c.Id == parentCommentId.Value && !c.IsDeleted);
            if (parentComment == null)
                throw new InvalidOperationException("父评论不存在或已删除");
        }

        var comment = new Comment(videoId, userId, content, parentCommentId);
        _context.Comments.Add(comment);
        video.ApplyCommentAdded();
        await _context.SaveChangesAsync();
        return comment;
    }

    public async Task<bool> DeleteCommentAsync(Guid commentId, Guid userId)
    {
        var comment = await _context.Comments.FirstOrDefaultAsync(c => c.Id == commentId && !c.IsDeleted);
        if (comment == null || comment.UserId != userId)
            return false;

        var video = await _context.Videos.FirstOrDefaultAsync(v => v.Id == comment.VideoId);
        if (video != null)
        {
            video.ApplyCommentRemoved();
        }

        comment.SoftDelete();
        await _context.SaveChangesAsync();
        return true;
    }
}

