using Application.DTOs;
using Domain.Videos;

namespace Application.Interfaces.Services;

public interface IVideoService
{
    Task<IEnumerable<VideoDto>> GetVideosAsync(int page = 1, int pageSize = 20, Guid? userId = null);
    Task<VideoDto?> GetVideoByIdAsync(Guid videoId, Guid? userId = null);
    Task<VideoDto> CreateVideoAsync(Guid uploaderUserId, string title, string videoUrl, string? description = null, string? thumbnailUrl = null, VideoVisibility visibility = VideoVisibility.Public);
    Task<VideoDto> UploadAndCreateVideoAsync(Guid uploaderUserId, UploadVideoRequest request, CancellationToken cancellationToken = default);
    Task<bool> UpdateVideoAsync(Guid videoId, Guid userId, string? title = null, string? description = null, string? thumbnailUrl = null, VideoVisibility? visibility = null);
    Task<bool> DeleteVideoAsync(Guid videoId, Guid userId);
    Task<bool> ToggleLikeAsync(Guid videoId, Guid userId);
    Task<bool> HasUserLikedAsync(Guid videoId, Guid userId);
    Task IncreaseViewCountAsync(Guid videoId);
    Task<IEnumerable<CommentDto>> GetVideoCommentsAsync(Guid videoId);
    Task<CommentDto> CreateCommentAsync(Guid videoId, Guid userId, string content, Guid? parentCommentId = null);
    Task<bool> DeleteCommentAsync(Guid commentId, Guid userId);
}

