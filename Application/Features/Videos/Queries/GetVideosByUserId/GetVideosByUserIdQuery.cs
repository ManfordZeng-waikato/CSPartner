using Application.Common.Interfaces;
using Application.DTOs.Video;

namespace Application.Features.Videos.Queries.GetVideosByUserId;

public record GetVideosByUserIdQuery(Guid UserId, Guid? CurrentUserId = null) : IQuery<IEnumerable<VideoDto>>;

