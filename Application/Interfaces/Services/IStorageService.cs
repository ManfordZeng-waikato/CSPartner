namespace Application.Interfaces.Services;

public interface IStorageService
{
    Task<string> UploadVideoAsync(Stream fileStream, string fileName, CancellationToken cancellationToken = default);
    Task<string> UploadThumbnailAsync(Stream fileStream, string fileName, CancellationToken cancellationToken = default);
    Task<bool> DeleteFileAsync(string objectKey, CancellationToken cancellationToken = default);
    string GetPublicUrl(string objectKey);
}
