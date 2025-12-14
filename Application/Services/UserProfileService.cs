using Application.Interfaces;
using Application.Interfaces.Repositories;
using Domain.Users;

namespace Application.Services;

public class UserProfileService : IUserProfileService
{
    private readonly IUserProfileRepository _userProfileRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UserProfileService(
        IUserProfileRepository userProfileRepository,
        IUnitOfWork unitOfWork)
    {
        _userProfileRepository = userProfileRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<UserProfile?> GetUserProfileAsync(Guid userId)
    {
        return await _userProfileRepository.GetByIdAsync(userId);
    }

    public async Task<UserProfile?> GetUserProfileByUserIdAsync(Guid userId)
    {
        return await _userProfileRepository.GetByUserIdAsync(userId);
    }

    public async Task<UserProfile> CreateOrUpdateUserProfileAsync(Guid userId, string? displayName, string? bio, string? avatarUrl, string? steamUrl, string? faceitUrl)
    {
        var profile = await _userProfileRepository.GetByUserIdAsync(userId);

        if (profile == null)
        {
            profile = new UserProfile(userId);
            await _userProfileRepository.AddAsync(profile);
        }

        profile.Update(displayName, bio, avatarUrl, steamUrl, faceitUrl);
        await _userProfileRepository.UpdateAsync(profile);
        await _unitOfWork.SaveChangesAsync();
        return profile;
    }
}
