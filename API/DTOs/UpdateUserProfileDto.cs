using System.ComponentModel.DataAnnotations;

namespace API.DTOs;

public class UpdateUserProfileDto
{
    [StringLength(50)]
    public string? DisplayName { get; set; }

    [StringLength(500)]
    public string? Bio { get; set; }

    public string? AvatarUrl { get; set; }

    public string? SteamProfileUrl { get; set; }

    public string? FaceitProfileUrl { get; set; }
}

