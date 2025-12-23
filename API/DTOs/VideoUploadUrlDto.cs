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
    public string UploadUrl { get; init; } = default!;
    public string ObjectKey { get; init; } = default!;
    public string PublicUrl { get; init; } = default!;
    public DateTime ExpiresAtUtc { get; init; }
    public string ContentType { get; init; } = default!;
    public string Method { get; init; } = "PUT";
}

