using Domain.Videos;

namespace Application.Interfaces.Repositories;

public interface IVideoRepository
{
    Task<IEnumerable<HighlightVideo>> GetVideosAsync(int page, int pageSize);
    Task<HighlightVideo?> GetVideoByIdAsync(Guid videoId);
    Task<HighlightVideo> AddAsync(HighlightVideo video);
    Task UpdateAsync(HighlightVideo video);
    Task<bool> ExistsAsync(Guid videoId);
    Task<VideoLike?> GetVideoLikeAsync(Guid videoId, Guid userId);
    Task AddVideoLikeAsync(VideoLike like);
    Task RemoveVideoLikeAsync(VideoLike like);
}
