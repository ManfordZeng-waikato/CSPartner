using Application.Common.Interfaces;
using Application.DTOs.Video;
using Domain.Videos;

namespace Application.Features.Videos.Commands.CreateVideo;

public record CreateVideoCommand(
    string VideoObjectKey,
    string? ThumbnailObjectKey = null,
    string Title = "",
    string? Description = null,
    VideoVisibility Visibility = VideoVisibility.Public) : ICommand<VideoDto>;

