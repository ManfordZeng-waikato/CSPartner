using Application.DTOs.UserProfile;
using Domain.Users;

namespace Application.Mappings;

public static class UserProfileMappings
{
    public static UserProfileDto ToDto(this UserProfile profile)
    {
        return new UserProfileDto
        {
            Id = profile.Id,
            UserId = profile.UserId,
            DisplayName = profile.DisplayName,
            Bio = profile.Bio,
            AvatarUrl = profile.AvatarUrl,
            SteamProfileUrl = profile.SteamProfileUrl,
            FaceitProfileUrl = profile.FaceitProfileUrl,
            CreatedAtUtc = profile.CreatedAtUtc,
            UpdatedAtUtc = profile.UpdatedAtUtc
        };
    }
}
