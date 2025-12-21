using Application.Common.Interfaces;
using Application.DTOs.UserProfile;

namespace Application.Features.UserProfiles.Commands.CreateOrUpdateUserProfile;

public record CreateOrUpdateUserProfileCommand(
    Guid UserId,
    string? DisplayName = null,
    string? Bio = null,
    string? AvatarUrl = null,
    string? SteamUrl = null,
    string? FaceitUrl = null,
    Guid? CurrentUserId = null) : ICommand<UserProfileDto>;

