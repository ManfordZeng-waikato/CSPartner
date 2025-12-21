using Application.Common.Interfaces;
using Microsoft.AspNetCore.Http;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Infrastructure.Identity;

public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public Guid? UserId
    {
        get
        {
            var user = _httpContextAccessor.HttpContext?.User;
            if (user?.Identity?.IsAuthenticated != true)
                return null;

            // Priority: JWT standard claim (sub) -> NameIdentifier -> "sub"
            var userIdClaim = user.FindFirst(JwtRegisteredClaimNames.Sub)
                              ?? user.FindFirst(ClaimTypes.NameIdentifier)
                              ?? user.FindFirst("sub");

            if (userIdClaim == null || string.IsNullOrWhiteSpace(userIdClaim.Value))
                return null;

            if (Guid.TryParse(userIdClaim.Value, out var userId))
                return userId;

            return null;
        }
    }

    public bool IsAuthenticated => _httpContextAccessor.HttpContext?.User?.Identity?.IsAuthenticated == true;
}

