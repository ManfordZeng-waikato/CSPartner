using Application.Common.Interfaces;
using Application.DTOs.Video;
using Domain.Videos;

namespace Application.Features.Videos.Commands.UploadAndCreateVideo;

public record UploadAndCreateVideoCommand(
    Stream VideoStream,
    string VideoFileName,
    string Title,
    string? Description = null,
    Stream? ThumbnailStream = null,
    string? ThumbnailFileName = null,
    VideoVisibility Visibility = VideoVisibility.Public) : ICommand<VideoDto>;

