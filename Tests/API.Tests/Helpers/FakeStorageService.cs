using Application.Common.Interfaces;
using Application.DTOs.Storage;

namespace API.Tests.Helpers;

public class FakeStorageService : IStorageService
{
    public PreSignedUploadResult Result { get; set; } = new(
        "https://upload.test/url",
        "videos/test/object.mp4",
        "https://public.test/object.mp4",
        DateTime.UtcNow.AddMinutes(15),
        "video/mp4");
    public bool FileExists { get; set; } = true;

    public Task<PreSignedUploadResult> GetVideoUploadUrlAsync(
        Guid userId,
        string fileName,
        string contentType,
        CancellationToken cancellationToken = default)
        => Task.FromResult(Result with { ContentType = contentType });

    public string GetPublicUrl(string objectKey) => $"https://public.test/{objectKey}";

    public Task<bool> FileExistsAsync(string objectKey, CancellationToken cancellationToken = default)
        => Task.FromResult(FileExists);
}
