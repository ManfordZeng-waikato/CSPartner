using Application.DTOs.UserProfile;

namespace Application.Interfaces.Services;

public interface IUserProfileService
{
    Task<UserProfileDto?> GetUserProfileAsync(Guid userId, Guid? currentUserId = null);
    Task<UserProfileDto?> GetUserProfileByUserIdAsync(Guid userId, Guid? currentUserId = null);
    Task<UserProfileDto> CreateOrUpdateUserProfileAsync(Guid userId, string? displayName, string? bio, string? avatarUrl, string? steamUrl, string? faceitUrl, Guid? currentUserId = null);
}

