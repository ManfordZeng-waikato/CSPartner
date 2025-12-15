using Application.DTOs;

namespace Application.Interfaces.Services;

public interface IUserProfileService
{
    Task<UserProfileDto?> GetUserProfileAsync(Guid userId);
    Task<UserProfileDto?> GetUserProfileByUserIdAsync(Guid userId);
    Task<UserProfileDto> CreateOrUpdateUserProfileAsync(Guid userId, string? displayName, string? bio, string? avatarUrl, string? steamUrl, string? faceitUrl);
}

