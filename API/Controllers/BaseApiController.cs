using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class BaseApiController : ControllerBase
{
    /// <summary>
    /// 从当前用户的 Claims 中提取用户 ID
    /// 优先使用 JWT 标准 claim (sub)，回退到 NameIdentifier
    /// 仅在已认证时尝试提取，返回 null 表示未认证或提取失败
    /// </summary>
    protected Guid? GetCurrentUserId()
    {
        if (User.Identity?.IsAuthenticated != true)
            return null;

        // 优先使用 JWT 标准 claim (sub)
        // JWT Bearer 中间件可能将 sub 映射到不同的 claim type
        var userIdClaim = User.FindFirst(JwtRegisteredClaimNames.Sub)
                          ?? User.FindFirst(ClaimTypes.NameIdentifier)
                          ?? User.FindFirst("sub");

        if (userIdClaim == null || string.IsNullOrWhiteSpace(userIdClaim.Value))
            return null;

        // 尝试解析为 Guid
        if (Guid.TryParse(userIdClaim.Value, out var userId))
            return userId;

        return null;
    }
}