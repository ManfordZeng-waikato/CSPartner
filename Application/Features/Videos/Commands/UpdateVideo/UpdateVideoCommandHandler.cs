using Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Videos.Commands.UpdateVideo;

public class UpdateVideoCommandHandler : IRequestHandler<UpdateVideoCommand, bool>
{
    private readonly IApplicationDbContext _context;

    public UpdateVideoCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<bool> Handle(UpdateVideoCommand request, CancellationToken cancellationToken)
    {
        var video = await _context.Videos
            .FirstOrDefaultAsync(v => v.Id == request.VideoId && !v.IsDeleted, cancellationToken);

        if (video == null || video.UploaderUserId != request.UserId)
            return false;

        if (request.Title != null)
            video.SetTitle(request.Title);

        if (request.Description != null)
            video.SetDescription(request.Description);

        if (request.ThumbnailUrl != null)
            video.SetThumbnailUrl(request.ThumbnailUrl);

        if (request.Visibility.HasValue)
            video.SetVisibility(request.Visibility.Value);

        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }
}

