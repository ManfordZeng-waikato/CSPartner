using Application.DTOs.UserProfile;
using Application.Interfaces;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Application.Mappings;
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

    public async Task<UserProfileDto?> GetUserProfileAsync(Guid userId)
    {
        var profile = await _userProfileRepository.GetByIdAsync(userId);
        return profile?.ToDto();
    }

    public async Task<UserProfileDto?> GetUserProfileByUserIdAsync(Guid userId)
    {
        var profile = await _userProfileRepository.GetByUserIdAsync(userId);
        return profile?.ToDto();
    }

    public async Task<UserProfileDto> CreateOrUpdateUserProfileAsync(Guid userId, string? displayName, string? bio, string? avatarUrl, string? steamUrl, string? faceitUrl)
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
        return profile.ToDto();
    }
}
