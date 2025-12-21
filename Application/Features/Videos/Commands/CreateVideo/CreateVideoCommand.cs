using Application.Common.Interfaces;
using Application.DTOs.Video;
using Domain.Videos;

namespace Application.Features.Videos.Commands.CreateVideo;

public record CreateVideoCommand(
    string Title,
    string VideoUrl,
    string? Description = null,
    string? ThumbnailUrl = null,
    VideoVisibility Visibility = VideoVisibility.Public) : ICommand<VideoDto>;

