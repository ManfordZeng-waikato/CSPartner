using Application.Common.Interfaces;
using Application.DTOs.Common;
using Application.DTOs.Video;

namespace Application.Features.Videos.Queries.GetVideos;

public record GetVideosQuery(string? Cursor = null, int PageSize = 20, Guid? CurrentUserId = null) : IQuery<CursorPagedResult<VideoDto>>;

