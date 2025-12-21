using Application.Common.Interfaces;
using Application.DTOs.UserProfile;

namespace Application.Features.UserProfiles.Commands.CreateOrUpdateUserProfile;

public record CreateOrUpdateUserProfileCommand(
    string? DisplayName = null,
    string? Bio = null,
    string? AvatarUrl = null,
    string? SteamUrl = null,
    string? FaceitUrl = null) : ICommand<UserProfileDto>;

