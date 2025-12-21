using Application.Common.Interfaces;
using Application.DTOs.Video;

namespace Application.Features.Videos.Queries.GetVideoById;

public record GetVideoByIdQuery(Guid VideoId, Guid? CurrentUserId = null) : IQuery<VideoDto?>;

