using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class BaseApiController : ControllerBase
{
    /// <summary>
    /// Extract user ID from current user's Claims
    /// Prefer JWT standard claim (sub), fallback to NameIdentifier
    /// Only attempts extraction when authenticated, returns null if not authenticated or extraction fails
    /// </summary>
    protected Guid? GetCurrentUserId()
    {
        if (User.Identity?.IsAuthenticated != true)
            return null;

        // Prefer JWT standard claim (sub)
        // JWT Bearer middleware may map sub to different claim type
        var userIdClaim = User.FindFirst(JwtRegisteredClaimNames.Sub)
                          ?? User.FindFirst(ClaimTypes.NameIdentifier)
                          ?? User.FindFirst("sub");

        if (userIdClaim == null || string.IsNullOrWhiteSpace(userIdClaim.Value))
            return null;

        // Try to parse as Guid
        if (Guid.TryParse(userIdClaim.Value, out var userId))
            return userId;

        return null;
    }
}