using Application.Common.Interfaces;
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
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<UserProfilesController> _logger;

    public UserProfilesController(
        IMediator mediator,
        ICurrentUserService currentUserService,
        ILogger<UserProfilesController> logger)
    {
        _mediator = mediator;
        _currentUserService = currentUserService;
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
        var profile = await _mediator.Send(new GetUserProfileByUserIdQuery(userId, _currentUserService.UserId));
        if (profile == null)
            return NotFound();

        return Ok(profile);
    }

    /// <summary>
    /// Update user profile
    /// </summary>
    [HttpPut]
    [Authorize]
    public async Task<ActionResult<UserProfileDto>> UpdateUserProfile([FromBody] UpdateUserProfileDto dto)
    {
        try
        {
            var command = new CreateOrUpdateUserProfileCommand(
                dto.DisplayName,
                dto.Bio,
                dto.AvatarUrl,
                dto.SteamProfileUrl,
                dto.FaceitProfileUrl);

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

