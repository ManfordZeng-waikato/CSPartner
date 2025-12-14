using Domain.Users;

namespace Application.Services;

public interface IUserProfileService
{
    Task<UserProfile?> GetUserProfileAsync(Guid userId);
    Task<UserProfile?> GetUserProfileByUserIdAsync(Guid userId);
    Task<UserProfile> CreateOrUpdateUserProfileAsync(Guid userId, string? displayName, string? bio, string? avatarUrl, string? steamUrl, string? faceitUrl);
}

