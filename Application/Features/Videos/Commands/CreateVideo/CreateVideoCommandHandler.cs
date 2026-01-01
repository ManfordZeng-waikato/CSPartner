using Application.Common.Interfaces;
using Application.DTOs.Video;
using Application.Mappings;
using Domain.Exceptions;
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
            throw AuthenticationRequiredException.ForOperation("create a video");

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

        return video.ToDto(false);
    }
}

