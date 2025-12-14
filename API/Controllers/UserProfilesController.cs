using Application.DTOs;
using Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

public class UserProfilesController : BaseApiController
{
    private readonly IUserProfileService _userProfileService;
    private readonly ILogger<UserProfilesController> _logger;

    public UserProfilesController(IUserProfileService userProfileService, ILogger<UserProfilesController> logger)
    {
        _userProfileService = userProfileService;
        _logger = logger;
    }

    /// <summary>
    /// 获取用户资料
    /// </summary>
    [HttpGet("{userId}")]
    public async Task<ActionResult<UserProfileDto>> GetUserProfile(Guid userId)
    {
        var profile = await _userProfileService.GetUserProfileByUserIdAsync(userId);
        if (profile == null)
            return NotFound();

        var dto = MapToDto(profile);
        return Ok(dto);
    }

    /// <summary>
    /// 更新用户资料
    /// </summary>
    [HttpPut("{userId}")]
    public async Task<ActionResult<UserProfileDto>> UpdateUserProfile(Guid userId, [FromBody] UpdateUserProfileDto dto)
    {
        try
        {
            var profile = await _userProfileService.CreateOrUpdateUserProfileAsync(
                userId,
                dto.DisplayName,
                dto.Bio,
                dto.AvatarUrl,
                dto.SteamProfileUrl,
                dto.FaceitProfileUrl);

            var profileDto = MapToDto(profile);
            return Ok(profileDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "更新用户资料失败");
            return BadRequest(new { error = ex.Message });
        }
    }

    private static UserProfileDto MapToDto(Domain.Users.UserProfile profile)
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

