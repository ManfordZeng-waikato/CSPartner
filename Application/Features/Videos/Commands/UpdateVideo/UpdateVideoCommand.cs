using Application.Common.Interfaces;
using Domain.Videos;

namespace Application.Features.Videos.Commands.UpdateVideo;

public record UpdateVideoCommand(
    Guid VideoId,
    string? Title = null,
    string? Description = null,
    string? ThumbnailUrl = null,
    VideoVisibility? Visibility = null) : ICommand<bool>;

