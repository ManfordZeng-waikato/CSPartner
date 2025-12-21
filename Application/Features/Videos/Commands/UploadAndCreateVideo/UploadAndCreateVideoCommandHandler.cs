using Application.DTOs.Video;
using Application.Features.Videos.Commands.CreateVideo;
using Application.Interfaces.Services;
using Domain.Videos;
using MediatR;

namespace Application.Features.Videos.Commands.UploadAndCreateVideo;

public class UploadAndCreateVideoCommandHandler : IRequestHandler<UploadAndCreateVideoCommand, VideoDto>
{
    private readonly IStorageService _storageService;
    private readonly IMediator _mediator;

    public UploadAndCreateVideoCommandHandler(
        IStorageService storageService,
        IMediator mediator)
    {
        _storageService = storageService;
        _mediator = mediator;
    }

    public async Task<VideoDto> Handle(UploadAndCreateVideoCommand request, CancellationToken cancellationToken)
    {
        ValidateVideoFile(request.VideoFileName);

        string videoUrl;
        using (request.VideoStream)
        {
            videoUrl = await _storageService.UploadVideoAsync(request.VideoStream, request.VideoFileName, cancellationToken);
        }

        string? thumbnailUrl = null;
        if (request.ThumbnailStream != null && !string.IsNullOrWhiteSpace(request.ThumbnailFileName))
        {
            ValidateThumbnailFile(request.ThumbnailFileName);
            using (request.ThumbnailStream)
            {
                thumbnailUrl = await _storageService.UploadThumbnailAsync(request.ThumbnailStream, request.ThumbnailFileName, cancellationToken);
            }
        }

        var createCommand = new CreateVideoCommand(
            request.Title,
            videoUrl,
            request.Description,
            thumbnailUrl,
            request.Visibility);

        return await _mediator.Send(createCommand, cancellationToken);
    }

    private static void ValidateVideoFile(string fileName)
    {
        var allowedVideoExtensions = new[] { ".mp4", ".webm", ".mov", ".avi" };
        var fileExtension = Path.GetExtension(fileName).ToLowerInvariant();
        if (!allowedVideoExtensions.Contains(fileExtension))
        {
            throw new ArgumentException("Unsupported video format. Supported: mp4, webm, mov, avi", nameof(fileName));
        }
    }

    private static void ValidateThumbnailFile(string fileName)
    {
        var allowedImageExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp" };
        var imageExtension = Path.GetExtension(fileName).ToLowerInvariant();
        if (!allowedImageExtensions.Contains(imageExtension))
        {
            throw new ArgumentException("Unsupported image format. Supported: jpg, jpeg, png, webp", nameof(fileName));
        }
    }
}

