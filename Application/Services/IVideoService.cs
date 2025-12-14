using Domain.Comments;
using Domain.Videos;

namespace Application.Services;

public interface IVideoService
{
    Task<IEnumerable<HighlightVideo>> GetVideosAsync(int page = 1, int pageSize = 20);
    Task<HighlightVideo?> GetVideoByIdAsync(Guid videoId);
    Task<HighlightVideo> CreateVideoAsync(Guid uploaderUserId, string title, string videoUrl, string? description = null, string? thumbnailUrl = null, VideoVisibility visibility = VideoVisibility.Public);
    Task<bool> UpdateVideoAsync(Guid videoId, Guid userId, string? title = null, string? description = null, string? thumbnailUrl = null, VideoVisibility? visibility = null);
    Task<bool> DeleteVideoAsync(Guid videoId, Guid userId);
    Task<bool> ToggleLikeAsync(Guid videoId, Guid userId);
    Task<bool> HasUserLikedAsync(Guid videoId, Guid userId);
    Task IncreaseViewCountAsync(Guid videoId);
    Task<IEnumerable<Comment>> GetVideoCommentsAsync(Guid videoId);
    Task<Comment> CreateCommentAsync(Guid videoId, Guid userId, string content, Guid? parentCommentId = null);
    Task<bool> DeleteCommentAsync(Guid commentId, Guid userId);
}

