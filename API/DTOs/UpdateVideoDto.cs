using System.ComponentModel.DataAnnotations;
using Domain.Videos;

namespace API.DTOs;

public class UpdateVideoDto
{
    [StringLength(120)]
    public string? Title { get; set; }

    [StringLength(2000)]
    public string? Description { get; set; }

    public string? ThumbnailUrl { get; set; }

    public VideoVisibility? Visibility { get; set; }
}

