using System.ComponentModel.DataAnnotations;
using Domain.Videos;

namespace Application.DTOs.Video;

public class CreateVideoDto
{
    [Required]
    [StringLength(120)]
    public string Title { get; set; } = default!;

    [StringLength(2000)]
    public string? Description { get; set; }

    [Required]
    public string VideoObjectKey { get; set; } = default!;

    public string? ThumbnailObjectKey { get; set; }

    public VideoVisibility Visibility { get; set; } = VideoVisibility.Public;

    [Required]
    [StringLength(50)]
    public string Map { get; set; } = default!;

    [Required]
    [StringLength(50)]
    public string Weapon { get; set; } = default!;
}
