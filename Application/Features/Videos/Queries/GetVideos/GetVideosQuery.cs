using Application.Common.Interfaces;
using Application.DTOs.Video;

namespace Application.Features.Videos.Queries.GetVideos;

public record GetVideosQuery(int Page = 1, int PageSize = 20, Guid? CurrentUserId = null) : IQuery<IEnumerable<VideoDto>>;

