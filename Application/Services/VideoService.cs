using Application.Interfaces;
using Application.Interfaces.Repositories;
using Domain.Comments;
using Domain.Exceptions;
using Domain.Videos;

namespace Application.Services;

public class VideoService : IVideoService
{
    private readonly IVideoRepository _videoRepository;
    private readonly ICommentRepository _commentRepository;
    private readonly IUnitOfWork _unitOfWork;

    public VideoService(
        IVideoRepository videoRepository,
        ICommentRepository commentRepository,
        IUnitOfWork unitOfWork)
    {
        _videoRepository = videoRepository;
        _commentRepository = commentRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<IEnumerable<HighlightVideo>> GetVideosAsync(int page = 1, int pageSize = 20)
    {
        return await _videoRepository.GetVideosAsync(page, pageSize);
    }

    public async Task<HighlightVideo?> GetVideoByIdAsync(Guid videoId)
    {
        return await _videoRepository.GetVideoByIdAsync(videoId);
    }

    public async Task<HighlightVideo> CreateVideoAsync(Guid uploaderUserId, string title, string videoUrl, string? description = null, string? thumbnailUrl = null, VideoVisibility visibility = VideoVisibility.Public)
    {
        var video = new HighlightVideo(uploaderUserId, title, videoUrl, description, thumbnailUrl);
        if (visibility != VideoVisibility.Public)
        {
            video.SetVisibility(visibility);
        }
        await _videoRepository.AddAsync(video);
        await _unitOfWork.SaveChangesAsync();
        return video;
    }

    public async Task<bool> UpdateVideoAsync(Guid videoId, Guid userId, string? title = null, string? description = null, string? thumbnailUrl = null, VideoVisibility? visibility = null)
    {
        var video = await _videoRepository.GetVideoByIdAsync(videoId);
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

        await _videoRepository.UpdateAsync(video);
        await _unitOfWork.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteVideoAsync(Guid videoId, Guid userId)
    {
        var video = await _videoRepository.GetVideoByIdAsync(videoId);
        if (video == null || video.UploaderUserId != userId)
            return false;

        video.SoftDelete();
        await _videoRepository.UpdateAsync(video);
        await _unitOfWork.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ToggleLikeAsync(Guid videoId, Guid userId)
    {
        var video = await _videoRepository.GetVideoByIdAsync(videoId);
        if (video == null)
            return false;

        var existingLike = await _videoRepository.GetVideoLikeAsync(videoId, userId);

        if (existingLike != null)
        {
            await _videoRepository.RemoveVideoLikeAsync(existingLike);
            video.ApplyLikeRemoved();
        }
        else
        {
            var like = new VideoLike(videoId, userId);
            await _videoRepository.AddVideoLikeAsync(like);
            video.ApplyLikeAdded();
        }

        await _videoRepository.UpdateAsync(video);
        await _unitOfWork.SaveChangesAsync();
        return true;
    }

    public async Task<bool> HasUserLikedAsync(Guid videoId, Guid userId)
    {
        var like = await _videoRepository.GetVideoLikeAsync(videoId, userId);
        return like != null;
    }

    public async Task IncreaseViewCountAsync(Guid videoId)
    {
        var video = await _videoRepository.GetVideoByIdAsync(videoId);
        if (video != null)
        {
            video.IncreaseView();
            await _videoRepository.UpdateAsync(video);
            await _unitOfWork.SaveChangesAsync();
        }
    }

    public async Task<IEnumerable<Comment>> GetVideoCommentsAsync(Guid videoId)
    {
        return await _commentRepository.GetCommentsByVideoIdAsync(videoId);
    }

    public async Task<Comment> CreateCommentAsync(Guid videoId, Guid userId, string content, Guid? parentCommentId = null)
    {
        var video = await _videoRepository.GetVideoByIdAsync(videoId);
        if (video == null)
            throw new VideoNotFoundException(videoId);

        if (parentCommentId.HasValue)
        {
            var parentComment = await _commentRepository.GetCommentByIdAsync(parentCommentId.Value);
            if (parentComment == null)
                throw new CommentNotFoundException(parentCommentId.Value);
        }

        var comment = new Comment(videoId, userId, content, parentCommentId);
        await _commentRepository.AddAsync(comment);
        video.ApplyCommentAdded();
        await _videoRepository.UpdateAsync(video);
        await _unitOfWork.SaveChangesAsync();
        return comment;
    }

    public async Task<bool> DeleteCommentAsync(Guid commentId, Guid userId)
    {
        var comment = await _commentRepository.GetCommentByIdAsync(commentId);
        if (comment == null || comment.UserId != userId)
            return false;

        var video = await _videoRepository.GetVideoByIdAsync(comment.VideoId);
        if (video != null)
        {
            video.ApplyCommentRemoved();
            await _videoRepository.UpdateAsync(video);
        }

        comment.SoftDelete();
        await _commentRepository.UpdateAsync(comment);
        await _unitOfWork.SaveChangesAsync();
        return true;
    }
}
