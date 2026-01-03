using Application.Common.Interfaces;
using Application.DTOs.Storage;

namespace Application.Features.Videos.Queries.GetVideoUploadUrl;

public record GetVideoUploadUrlQuery(
    string FileName,
    string? ContentType = null) : IQuery<PreSignedUploadResult>;

