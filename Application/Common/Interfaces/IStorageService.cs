using Application.DTOs.Storage;

namespace Application.Common.Interfaces;

public interface IStorageService
{
    /// <summary>
    /// Generate a pre-signed URL for uploading a video file directly to object storage.
    /// </summary>
    Task<PreSignedUploadResult> GetVideoUploadUrlAsync(
        string fileName,
        string contentType,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Build the public URL from an object key.
    /// </summary>
    string GetPublicUrl(string objectKey);

    /// <summary>
    /// Check if a file exists in object storage by object key.
    /// </summary>
    Task<bool> FileExistsAsync(string objectKey, CancellationToken cancellationToken = default);
}

