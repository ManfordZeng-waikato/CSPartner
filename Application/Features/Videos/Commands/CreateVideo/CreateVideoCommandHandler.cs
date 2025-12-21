using Application.Common.Interfaces;
using Application.DTOs.Video;
using Application.Mappings;
using Domain.Videos;
using MediatR;

namespace Application.Features.Videos.Commands.CreateVideo;

public class CreateVideoCommandHandler : IRequestHandler<CreateVideoCommand, VideoDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public CreateVideoCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<VideoDto> Handle(CreateVideoCommand request, CancellationToken cancellationToken)
    {
        if (!_currentUserService.UserId.HasValue)
            throw new UnauthorizedAccessException("User must be authenticated to create a video");

        var video = new HighlightVideo(
            _currentUserService.UserId.Value,
            request.Title,
            request.VideoUrl,
            request.Description,
            request.ThumbnailUrl);

        if (request.Visibility != VideoVisibility.Public)
        {
            video.SetVisibility(request.Visibility);
        }

        await _context.Videos.AddAsync(video, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        return video.ToDto(false);
    }
}

