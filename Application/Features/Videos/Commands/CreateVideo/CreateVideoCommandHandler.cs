using Application.Common.Interfaces;
using Application.DTOs.Video;
using Application.Mappings;
using Domain.Videos;
using MediatR;

namespace Application.Features.Videos.Commands.CreateVideo;

public class CreateVideoCommandHandler : IRequestHandler<CreateVideoCommand, VideoDto>
{
    private readonly IApplicationDbContext _context;

    public CreateVideoCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<VideoDto> Handle(CreateVideoCommand request, CancellationToken cancellationToken)
    {
        var video = new HighlightVideo(
            request.UploaderUserId,
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

