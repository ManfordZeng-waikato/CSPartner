using Application.Common.Interfaces;
using Application.DTOs.Video;
using Application.Mappings;
using Domain.Exceptions;
using Domain.Videos;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Features.Videos.Commands.CreateVideo;

public class CreateVideoCommandHandler : IRequestHandler<CreateVideoCommand, VideoDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly IStorageService _storageService;
    private readonly ILogger<CreateVideoCommandHandler> _logger;

    public CreateVideoCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService,
        IStorageService storageService,
        ILogger<CreateVideoCommandHandler> logger)
    {
        _context = context;
        _currentUserService = currentUserService;
        _storageService = storageService;
        _logger = logger;
    }

    public async Task<VideoDto> Handle(CreateVideoCommand request, CancellationToken cancellationToken)
    {
        if (!_currentUserService.UserId.HasValue)
            throw AuthenticationRequiredException.ForOperation("create a video");

        // Business validation: Verify that the objectKey belongs to the current user
        // Format: videos/{userId}/{yyyyMMdd}/highlight-{guid}.mp4
        var expectedPrefix = $"videos/{_currentUserService.UserId.Value}/";
        if (!request.VideoObjectKey.StartsWith(expectedPrefix, StringComparison.OrdinalIgnoreCase))
        {
            _logger.LogWarning(
                "Attempted to create video record with objectKey that doesn't belong to user. UserId: {UserId}, ObjectKey: {ObjectKey}",
                _currentUserService.UserId.Value,
                request.VideoObjectKey);
            throw InvalidObjectKeyException.ForVideo(request.VideoObjectKey);
        }

        // Business validation: Verify that the video file exists in R2 before creating database record
        // This prevents orphaned database records if R2 upload failed
        var videoExists = await _storageService.FileExistsAsync(request.VideoObjectKey, cancellationToken);
        if (!videoExists)
        {
            _logger.LogWarning("Attempted to create video record for non-existent file: {ObjectKey}", request.VideoObjectKey);
            throw new StorageFileNotFoundException(request.VideoObjectKey);
        }

        // Verify thumbnail if provided
        if (!string.IsNullOrWhiteSpace(request.ThumbnailObjectKey))
        {
            // Verify that the thumbnail objectKey belongs to the current user
            if (!request.ThumbnailObjectKey.StartsWith(expectedPrefix, StringComparison.OrdinalIgnoreCase))
            {
                _logger.LogWarning(
                    "Attempted to create video record with thumbnail objectKey that doesn't belong to user. UserId: {UserId}, ThumbnailObjectKey: {ThumbnailObjectKey}",
                    _currentUserService.UserId.Value,
                    request.ThumbnailObjectKey);
                throw InvalidObjectKeyException.ForThumbnail(request.ThumbnailObjectKey);
            }

            var thumbnailExists = await _storageService.FileExistsAsync(request.ThumbnailObjectKey, cancellationToken);
            if (!thumbnailExists)
            {
                _logger.LogWarning("Thumbnail file not found: {ObjectKey}", request.ThumbnailObjectKey);
                // Don't fail the request if thumbnail is missing, just log it
            }
        }

        // Build URLs from object keys
        var videoUrl = _storageService.GetPublicUrl(request.VideoObjectKey);
        string? thumbnailUrl = null;
        if (!string.IsNullOrWhiteSpace(request.ThumbnailObjectKey))
        {
            thumbnailUrl = _storageService.GetPublicUrl(request.ThumbnailObjectKey);
        }

        // Create video entity
        var video = new HighlightVideo(
            _currentUserService.UserId.Value,
            request.Title,
            videoUrl,
            request.Description,
            thumbnailUrl);

        if (request.Visibility != VideoVisibility.Public)
        {
            video.SetVisibility(request.Visibility);
        }

        await _context.Videos.AddAsync(video, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        return video.ToDto(false);
    }
}

