using System.ComponentModel.DataAnnotations;
using Domain.Videos;

namespace API.DTOs;

public class UploadVideoFormRequest
{
    [Required(ErrorMessage = "Video file cannot be empty")]
    public IFormFile VideoFile { get; set; } = default!;

    [Required(ErrorMessage = "Video title cannot be empty")]
    [StringLength(120, ErrorMessage = "Title cannot exceed 120 characters")]
    public string Title { get; set; } = default!;

    [StringLength(2000, ErrorMessage = "Description cannot exceed 2000 characters")]
    public string? Description { get; set; }

    public IFormFile? ThumbnailFile { get; set; }

    public VideoVisibility Visibility { get; set; } = VideoVisibility.Public;
}
