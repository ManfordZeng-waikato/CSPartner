using Application.DTOs.Auth;
using Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace API.Controllers;

[AllowAnonymous]
public class AccountController : BaseApiController
{
    private readonly IAuthService _authService;
    private readonly ILogger<AccountController> _logger;

    public AccountController(IAuthService authService, ILogger<AccountController> logger)
    {
        _authService = authService;
        _logger = logger;
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
        // Challenge without custom properties - let OAuth middleware handle state cookies
        // The CallbackPath is configured in AddGitHub, and the callback handler will
        // redirect to the frontend after processing
        return Challenge("GitHub");
    }

    /// <summary>
    /// Handle GitHub OAuth callback - processes authentication and redirects to frontend
    /// This endpoint is called after OAuth middleware processes the callback at /signin-github
    /// </summary>
    [HttpGet("github-callback")]
    public async Task<IActionResult> GitHubCallback()
    {
        var config = HttpContext.RequestServices.GetRequiredService<IConfiguration>();
        var clientUrl = config["ClientApp:ClientUrl"] ?? $"{Request.Scheme}://{Request.Host}";

        try
        {
            // Authenticate using the GitHub scheme (OAuth middleware should have set this)
            var authenticateResult = await HttpContext.AuthenticateAsync("GitHub");
            
            if (!authenticateResult.Succeeded)
            {
                _logger.LogWarning("GitHub authentication failed: {FailureMessage}", 
                    authenticateResult.Failure?.Message ?? "Unknown error");
                // GitHub scheme doesn't support SignOut - it's only for OAuth flow
                // Just redirect on failure
                return Redirect($"{clientUrl}/login?error=github_auth_failed");
            }

            var claims = authenticateResult.Principal?.Claims;
            if (claims == null)
            {
                _logger.LogWarning("GitHub authentication succeeded but no claims found");
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

            // Note: GitHub scheme doesn't need to be signed out - it's only used temporarily
            // for OAuth flow. The OAuth middleware handles cleanup automatically.

            if (string.IsNullOrWhiteSpace(githubId))
            {
                _logger.LogWarning("GitHub ID is missing from claims");
                return Redirect($"{clientUrl}/login?error=github_id_missing");
            }

            if (string.IsNullOrWhiteSpace(email))
            {
                _logger.LogWarning("GitHub email is missing from claims");
                return Redirect($"{clientUrl}/login?error=github_email_required");
            }

            // Process GitHub login
            var result = await _authService.LoginWithGitHubAsync(email, name ?? email, avatarUrl, githubId);

            if (!result.Succeeded)
            {
                var errorMessage = result.Errors?.FirstOrDefault() ?? "GitHub login failed";
                _logger.LogWarning("GitHub login failed: {Error}", errorMessage);
                return Redirect($"{clientUrl}/login?error={Uri.EscapeDataString(errorMessage)}");
            }

            // Redirect to frontend with token in query string
            return Redirect($"{clientUrl}/auth/callback?token={Uri.EscapeDataString(result.Token ?? string.Empty)}&userId={result.UserId}&email={Uri.EscapeDataString(result.Email ?? string.Empty)}&displayName={Uri.EscapeDataString(result.DisplayName ?? string.Empty)}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during GitHub OAuth callback handling");
            // GitHub scheme doesn't support SignOut - just redirect on error
            return Redirect($"{clientUrl}/login?error=github_auth_error");
        }
    }
}
