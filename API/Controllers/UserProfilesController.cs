using Application.DTOs;
using Application.Interfaces.Services;
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

        return Ok(profile);
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

            return Ok(profile);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "更新用户资料失败");
            return BadRequest(new { error = ex.Message });
        }
    }
}

