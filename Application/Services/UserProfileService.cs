using Application.DTOs.UserProfile;
using Application.DTOs.Video;
using Application.Interfaces;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Application.Mappings;
using Domain.Users;

namespace Application.Services;

public class UserProfileService : IUserProfileService
{
    private readonly IUserProfileRepository _userProfileRepository;
    private readonly IVideoService _videoService;
    private readonly IUnitOfWork _unitOfWork;

    public UserProfileService(
        IUserProfileRepository userProfileRepository,
        IVideoService videoService,
        IUnitOfWork unitOfWork)
    {
        _userProfileRepository = userProfileRepository;
        _videoService = videoService;
        _unitOfWork = unitOfWork;
    }

    public async Task<UserProfileDto?> GetUserProfileAsync(Guid userId, Guid? currentUserId = null)
    {
        var profile = await _userProfileRepository.GetByIdAsync(userId);
        if (profile == null)
            return null;

        // Use VideoService to get videos with visibility filtering
        var videoDtos = await _videoService.GetVideosByUserIdAsync(profile.UserId, currentUserId);
        return profile.ToDto(videoDtos.ToList());
    }

    public async Task<UserProfileDto?> GetUserProfileByUserIdAsync(Guid userId, Guid? currentUserId = null)
    {
        var profile = await _userProfileRepository.GetByUserIdAsync(userId);
        if (profile == null)
            return null;

        // Use VideoService to get videos with visibility filtering
        var videoDtos = await _videoService.GetVideosByUserIdAsync(userId, currentUserId);
        return profile.ToDto(videoDtos.ToList());
    }

    public async Task<UserProfileDto> CreateOrUpdateUserProfileAsync(Guid userId, string? displayName, string? bio, string? avatarUrl, string? steamUrl, string? faceitUrl, Guid? currentUserId = null)
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

        // Use VideoService to get videos with visibility filtering
        var videoDtos = await _videoService.GetVideosByUserIdAsync(userId, currentUserId ?? userId);
        return profile.ToDto(videoDtos.ToList());
    }
}
