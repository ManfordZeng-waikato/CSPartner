namespace Application.Services;

public interface IR2StorageService
{
    Task<string> UploadVideoAsync(Stream fileStream, string fileName, CancellationToken cancellationToken = default);
    Task<string> UploadThumbnailAsync(Stream fileStream, string fileName, CancellationToken cancellationToken = default);
    Task<bool> DeleteFileAsync(string objectKey, CancellationToken cancellationToken = default);
    string GetPublicUrl(string objectKey);
}

