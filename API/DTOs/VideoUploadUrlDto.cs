using System.ComponentModel.DataAnnotations;

namespace API.DTOs;

public class VideoUploadUrlRequest
{
    [Required]
    public string FileName { get; set; } = default!;

    [Required]
    public string ContentType { get; set; } = "video/mp4";
}

public class VideoUploadUrlResponse
{
    /// <summary>
    /// Pre-signed URL for uploading the video file directly to R2 storage.
    /// </summary>
    public string UploadUrl { get; init; } = default!;
    
    /// <summary>
    /// Object key in R2 storage. Use this when creating the video record.
    /// </summary>
    public string ObjectKey { get; init; } = default!;
    
    /// <summary>
    /// Public URL for accessing the uploaded video.
    /// </summary>
    public string PublicUrl { get; init; } = default!;
    
    /// <summary>
    /// UTC timestamp when the pre-signed URL expires.
    /// </summary>
    public DateTime ExpiresAtUtc { get; init; }
    
    /// <summary>
    /// REQUIRED: The Content-Type header value that MUST be used when uploading to UploadUrl.
    /// The pre-signed URL signature includes this Content-Type, so the upload request MUST include
    /// the exact same Content-Type header, otherwise the upload will fail with a signature mismatch error.
    /// Example: "video/mp4", "video/webm", "video/quicktime", "video/x-msvideo"
    /// </summary>
    public string ContentType { get; init; } = default!;
    
    /// <summary>
    /// HTTP method to use when uploading. Always "PUT" for video uploads.
    /// </summary>
    public string Method { get; init; } = "PUT";
}

