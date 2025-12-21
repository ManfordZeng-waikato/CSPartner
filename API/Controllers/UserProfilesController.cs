using Application.DTOs.UserProfile;
using Application.Features.UserProfiles.Commands.CreateOrUpdateUserProfile;
using Application.Features.UserProfiles.Queries.GetUserProfileByUserId;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace API.Controllers;

public class UserProfilesController : BaseApiController
{
    private readonly IMediator _mediator;
    private readonly ILogger<UserProfilesController> _logger;

    public UserProfilesController(IMediator mediator, ILogger<UserProfilesController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Get user profile
    /// Visibility rules for videos:
    /// - Anonymous users: only Public videos
    /// - Viewing own profile: all videos (Public + Private)
    /// - Viewing others' profile: only Public videos
    /// </summary>
    [HttpGet("{userId}")]
    [AllowAnonymous]
    public async Task<ActionResult<UserProfileDto>> GetUserProfile(Guid userId)
    {
        var currentUserId = GetCurrentUserId();
        var profile = await _mediator.Send(new GetUserProfileByUserIdQuery(userId, currentUserId));
        if (profile == null)
            return NotFound();

        return Ok(profile);
    }

    /// <summary>
    /// Update user profile
    /// </summary>
    [HttpPut("{userId}")]
    [Authorize]
    public async Task<ActionResult<UserProfileDto>> UpdateUserProfile(Guid userId, [FromBody] UpdateUserProfileDto dto)
    {
        var currentUserId = GetCurrentUserId();
        if (!currentUserId.HasValue || currentUserId.Value != userId)
        {
            return Forbid(); // Only allow users to update their own profile
        }

        try
        {
            var command = new CreateOrUpdateUserProfileCommand(
                userId,
                dto.DisplayName,
                dto.Bio,
                dto.AvatarUrl,
                dto.SteamProfileUrl,
                dto.FaceitProfileUrl,
                currentUserId);

            var profile = await _mediator.Send(command);

            return Ok(profile);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update user profile");
            return BadRequest(new { error = ex.Message });
        }
    }
}

