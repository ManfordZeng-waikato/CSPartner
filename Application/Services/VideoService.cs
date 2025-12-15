using Application.DTOs;
using Application.Interfaces;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Application.Mappings;
using Domain.Comments;
using Domain.Exceptions;
using Domain.Videos;

namespace Application.Services;

public class VideoService : IVideoService
{
    private readonly IVideoRepository _videoRepository;
    private readonly ICommentRepository _commentRepository;
    private readonly IStorageService _storageService;
    private readonly IUnitOfWork _unitOfWork;

    public VideoService(
        IVideoRepository videoRepository,
        ICommentRepository commentRepository,
        IStorageService storageService,
        IUnitOfWork unitOfWork)
    {
        _videoRepository = videoRepository;
        _commentRepository = commentRepository;
        _storageService = storageService;
        _unitOfWork = unitOfWork;
    }

    public async Task<IEnumerable<VideoDto>> GetVideosAsync(int page = 1, int pageSize = 20, Guid? userId = null)
    {
        var videos = await _videoRepository.GetVideosAsync(page, pageSize);
        var videoDtos = new List<VideoDto>();

        foreach (var video in videos)
        {
            bool hasLiked = false;
            if (userId.HasValue)
            {
                hasLiked = await HasUserLikedAsync(video.VideoId, userId.Value);
            }
            videoDtos.Add(video.ToDto(hasLiked));
        }

        return videoDtos;
    }

    public async Task<VideoDto?> GetVideoByIdAsync(Guid videoId, Guid? userId = null)
    {
        var video = await _videoRepository.GetVideoByIdAsync(videoId);
        if (video == null)
            return null;

        bool hasLiked = false;
        if (userId.HasValue)
        {
            hasLiked = await HasUserLikedAsync(videoId, userId.Value);
        }

        return video.ToDto(hasLiked);
    }

    public async Task<VideoDto> CreateVideoAsync(Guid uploaderUserId, string title, string videoUrl, string? description = null, string? thumbnailUrl = null, VideoVisibility visibility = VideoVisibility.Public)
    {
        var video = new HighlightVideo(uploaderUserId, title, videoUrl, description, thumbnailUrl);
        if (visibility != VideoVisibility.Public)
        {
            video.SetVisibility(visibility);
        }
        await _videoRepository.AddAsync(video);
        await _unitOfWork.SaveChangesAsync();
        return video.ToDto(false);
    }

    public async Task<VideoDto> UploadAndCreateVideoAsync(Guid uploaderUserId, UploadVideoRequest request, CancellationToken cancellationToken = default)
    {
        // Validate video file
        ValidateVideoFile(request.VideoFileName);

        // Upload video to storage
        string videoUrl;
        using (request.VideoStream)
        {
            videoUrl = await _storageService.UploadVideoAsync(request.VideoStream, request.VideoFileName, cancellationToken);
        }

        // Upload thumbnail (if provided)
        string? thumbnailUrl = null;
        if (request.ThumbnailStream != null && !string.IsNullOrWhiteSpace(request.ThumbnailFileName))
        {
            ValidateThumbnailFile(request.ThumbnailFileName);
            using (request.ThumbnailStream)
            {
                thumbnailUrl = await _storageService.UploadThumbnailAsync(request.ThumbnailStream, request.ThumbnailFileName, cancellationToken);
            }
        }

        // Create video record
        return await CreateVideoAsync(
            uploaderUserId,
            request.Title,
            videoUrl,
            request.Description,
            thumbnailUrl,
            request.Visibility);
    }

    private static void ValidateVideoFile(string fileName)
    {
        var allowedVideoExtensions = new[] { ".mp4", ".webm", ".mov", ".avi" };
        var fileExtension = Path.GetExtension(fileName).ToLowerInvariant();
        if (!allowedVideoExtensions.Contains(fileExtension))
        {
            throw new ArgumentException("不支持的视频格式，支持: mp4, webm, mov, avi", nameof(fileName));
        }
    }

    private static void ValidateThumbnailFile(string fileName)
    {
        var allowedImageExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp" };
        var imageExtension = Path.GetExtension(fileName).ToLowerInvariant();
        if (!allowedImageExtensions.Contains(imageExtension))
        {
            throw new ArgumentException("不支持的图片格式，支持: jpg, jpeg, png, webp", nameof(fileName));
        }
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

    public async Task<IEnumerable<CommentDto>> GetVideoCommentsAsync(Guid videoId)
    {
        var comments = await _commentRepository.GetCommentsByVideoIdAsync(videoId);
        return comments.Select(c => c.ToDto());
    }

    public async Task<CommentDto> CreateCommentAsync(Guid videoId, Guid userId, string content, Guid? parentCommentId = null)
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
        return comment.ToDto();
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
