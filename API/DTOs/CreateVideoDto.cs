using System.ComponentModel.DataAnnotations;
using Domain.Videos;

namespace API.DTOs;

public class CreateVideoDto
{
    [Required]
    [StringLength(120)]
    public string Title { get; set; } = default!;

    [StringLength(2000)]
    public string? Description { get; set; }

    [Required]
    public string VideoUrl { get; set; } = default!;

    public string? ThumbnailUrl { get; set; }

    public VideoVisibility Visibility { get; set; } = VideoVisibility.Public;
}

