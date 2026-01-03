using Application.Common.Interfaces;
using Domain.Videos;

namespace Application.Features.Videos.Commands.UpdateVideo;

public record UpdateVideoCommand(
    Guid VideoId,
    VideoVisibility Visibility) : ICommand<bool>;

