using Application.Common.Interfaces;
using Application.DTOs.Storage;
using Domain.Exceptions;
using MediatR;

namespace Application.Features.Videos.Queries.GetVideoUploadUrl;

public class GetVideoUploadUrlQueryHandler : IRequestHandler<GetVideoUploadUrlQuery, PreSignedUploadResult>
{
    private readonly IStorageService _storageService;
    private readonly ICurrentUserService _currentUserService;

    public GetVideoUploadUrlQueryHandler(
        IStorageService storageService,
        ICurrentUserService currentUserService)
    {
        _storageService = storageService;
        _currentUserService = currentUserService;
    }

    public async Task<PreSignedUploadResult> Handle(GetVideoUploadUrlQuery request, CancellationToken cancellationToken)
    {
        if (!_currentUserService.UserId.HasValue)
            throw AuthenticationRequiredException.ForOperation("get upload URL");

        var contentType = string.IsNullOrWhiteSpace(request.ContentType)
            ? "video/mp4"
            : request.ContentType;

        return await _storageService.GetVideoUploadUrlAsync(
            _currentUserService.UserId.Value,
            request.FileName,
            contentType,
            cancellationToken);
    }
}

