namespace Application.DTOs;

public class UploadVideoResponseDto
{
    public string VideoUrl { get; set; } = default!;
    public string? ThumbnailUrl { get; set; }
    public string Message { get; set; } = default!;
}
