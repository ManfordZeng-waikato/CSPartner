using Application.DTOs.Auth;
using Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Configuration;

namespace API.Controllers;

[AllowAnonymous]
public class AccountController : BaseApiController
{
    private readonly IAuthService _authService;

    public AccountController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterDto dto)
    {
        var result = await _authService.RegisterAsync(dto);
        if (!result.Succeeded)
        {
            return BadRequest(new { errors = result.Errors });
        }

        return Ok(result);
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto dto)
    {
        var result = await _authService.LoginAsync(dto);
        
        // Always return HTTP 200 with succeeded: false in the body for failed attempts
        // This allows the client to properly handle special cases like EMAIL_NOT_CONFIRMED
        // where credentials are valid but email is not confirmed - this is a workflow state,
        // not an authentication failure, so we return 200 to allow client-side handling
        if (!result.Succeeded)
        {
            return Ok(result);
        }

        return Ok(result);
    }

    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        await _authService.LogoutAsync();
        return Ok(new { succeeded = true });
    }

    /// <summary>
    /// Resend email confirmation link
    /// </summary>
    [HttpGet("resendConfirmationEmail")]
    public async Task<IActionResult> ResendConfirmationEmail(string email)
    {
        var (succeeded, message) = await _authService.ResendConfirmationEmailAsync(email);
        
        if (!succeeded)
        {
            return BadRequest(new { error = message });
        }

        return Ok(new { message });
    }

    /// <summary>
    /// Confirm email address
    /// </summary>
    [HttpPost("confirmEmail")]
    public async Task<IActionResult> ConfirmEmail([FromQuery] Guid userId, [FromQuery] string code)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            return BadRequest(new { error = "Confirmation code is required" });
        }

        var result = await _authService.ConfirmEmailAsync(userId, code);
        
        if (!result.Succeeded)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// Request password reset - sends reset code to user's email
    /// </summary>
    [HttpPost("requestPasswordReset")]
    public async Task<IActionResult> RequestPasswordReset([FromBody] RequestPasswordResetDto dto)
    {
        var (succeeded, message) = await _authService.RequestPasswordResetAsync(dto);
        
        if (!succeeded)
        {
            return BadRequest(new { error = message });
        }

        return Ok(new { message });
    }

    /// <summary>
    /// Reset password using reset code
    /// </summary>
    [HttpPost("resetPassword")]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto dto)
    {
        var (succeeded, message) = await _authService.ResetPasswordAsync(dto);
        
        if (!succeeded)
        {
            return BadRequest(new { error = message });
        }

        return Ok(new { message });
    }

    /// <summary>
    /// Initiate GitHub OAuth login - redirects to GitHub
    /// </summary>
    [HttpGet("github-login")]
    public IActionResult GitHubLogin()
    {
        var properties = new Microsoft.AspNetCore.Authentication.AuthenticationProperties 
        { 
            RedirectUri = "/api/account/github-callback-handler"
        };
        return Challenge(properties, "GitHub");
    }

    /// <summary>
    /// Handle GitHub OAuth callback - processes authentication and redirects to frontend
    /// </summary>
    [HttpGet("github-callback-handler")]
    public async Task<IActionResult> GitHubCallbackHandler()
    {
        var authenticateResult = await HttpContext.AuthenticateAsync("GitHub");
        
        if (!authenticateResult.Succeeded)
        {
            var config = HttpContext.RequestServices.GetRequiredService<IConfiguration>();
            var clientUrl = config["ClientApp:ClientUrl"] ?? $"{Request.Scheme}://{Request.Host}";
            return Redirect($"{clientUrl}/login?error=github_auth_failed");
        }

        var claims = authenticateResult.Principal?.Claims;
        if (claims == null)
        {
            var config = HttpContext.RequestServices.GetRequiredService<IConfiguration>();
            var clientUrl = config["ClientApp:ClientUrl"] ?? $"{Request.Scheme}://{Request.Host}";
            return Redirect($"{clientUrl}/login?error=github_auth_failed");
        }

        // Extract GitHub user information
        var githubId = claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value
            ?? claims.FirstOrDefault(c => c.Type == "sub")?.Value
            ?? claims.FirstOrDefault(c => c.Type == "id")?.Value;
        var email = claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value
            ?? claims.FirstOrDefault(c => c.Type == "email")?.Value
            ?? claims.FirstOrDefault(c => c.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress")?.Value;
        var name = claims.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value
            ?? claims.FirstOrDefault(c => c.Type == "name")?.Value
            ?? claims.FirstOrDefault(c => c.Type == "login")?.Value
            ?? claims.FirstOrDefault(c => c.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name")?.Value;
        var avatarUrl = claims.FirstOrDefault(c => c.Type == "avatar_url")?.Value
            ?? claims.FirstOrDefault(c => c.Type == "picture")?.Value
            ?? claims.FirstOrDefault(c => c.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/uri")?.Value;

        var config2 = HttpContext.RequestServices.GetRequiredService<IConfiguration>();
        var clientUrl2 = config2["ClientApp:ClientUrl"] ?? $"{Request.Scheme}://{Request.Host}";

        if (string.IsNullOrWhiteSpace(githubId))
        {
            return Redirect($"{clientUrl2}/login?error=github_id_missing");
        }

        if (string.IsNullOrWhiteSpace(email))
        {
            return Redirect($"{clientUrl2}/login?error=github_email_required");
        }

        // Note: We don't need to sign out the GitHub scheme as it's only used temporarily
        // for OAuth flow. We'll use JWT tokens for actual authentication.

        // Process GitHub login
        var result = await _authService.LoginWithGitHubAsync(email, name ?? email, avatarUrl, githubId);

        if (!result.Succeeded)
        {
            var errorMessage = result.Errors?.FirstOrDefault() ?? "GitHub login failed";
            return Redirect($"{clientUrl2}/login?error={Uri.EscapeDataString(errorMessage)}");
        }

        // Redirect to frontend with token in query string
        return Redirect($"{clientUrl2}/auth/callback?token={Uri.EscapeDataString(result.Token ?? string.Empty)}&userId={result.UserId}&email={Uri.EscapeDataString(result.Email ?? string.Empty)}&displayName={Uri.EscapeDataString(result.DisplayName ?? string.Empty)}");
    }
}
