using Application.DTOs.Auth;
using Application.Common.Interfaces;
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
    [AllowAnonymous]
    [HttpGet("github-login")]
    public IActionResult GitHubLogin()
    {
        // Set ReturnUrl so OAuth middleware redirects to our callback handler after authentication
        var config = HttpContext.RequestServices.GetRequiredService<IConfiguration>();
        var clientUrl = config["ClientApp:ClientUrl"] ?? $"{Request.Scheme}://{Request.Host}";
        var returnUrl = $"{clientUrl}/api/account/github-callback";
        
        var properties = new AuthenticationProperties { RedirectUri = returnUrl };
        return Challenge(properties, "GitHub");
    }


    /// <summary>
    /// Handle GitHub OAuth callback - processes authentication and redirects to frontend
    /// This endpoint is called after OAuth middleware processes the callback at /signin-github
    /// The OAuth middleware automatically handles /signin-github and signs in to External scheme,
    /// then redirects here for business logic processing
    /// </summary>
    [AllowAnonymous]
    [HttpGet("github-callback")]
    public async Task<IActionResult> GitHubCallback()
    {
        var config = HttpContext.RequestServices.GetRequiredService<IConfiguration>();
        var clientUrl = config["ClientApp:ClientUrl"] ?? $"{Request.Scheme}://{Request.Host}";

        try
        {
            // Read GitHub authentication result from External Cookie
            var result = await HttpContext.AuthenticateAsync("External");

            if (!result.Succeeded || result.Principal == null)
            {
                _logger.LogWarning("Failed to read External Cookie in GitHub callback: {Message}",
                    result.Failure?.Message ?? "Principal is null");
                return Redirect($"{clientUrl}/login?error=github_auth_failed");
            }

            var claims = result.Principal.Claims.ToList();

            // GitHub user unique identifier (most reliable)
            var githubId =
                claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value
                ?? claims.FirstOrDefault(c => c.Type == "sub")?.Value
                ?? claims.FirstOrDefault(c => c.Type == "id")?.Value;

            var name =
                claims.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value
                ?? claims.FirstOrDefault(c => c.Type == "name")?.Value
                ?? claims.FirstOrDefault(c => c.Type == "login")?.Value;

            var avatarUrl =
                claims.FirstOrDefault(c => c.Type == "avatar_url")?.Value
                ?? claims.FirstOrDefault(c => c.Type == "picture")?.Value;

            // Email may not be available (GitHub allows users to hide it), try claims first
            var email =
                claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value
                ?? claims.FirstOrDefault(c => c.Type == "email")?.Value;

            if (string.IsNullOrWhiteSpace(githubId))
            {
                _logger.LogWarning("GitHub callback missing githubId");
                await HttpContext.SignOutAsync("External");
                return Redirect($"{clientUrl}/login?error=github_id_missing");
            }

            // Email is required for this application
            // Note: GitHub may not provide email if user has hidden it or scope is insufficient
            if (string.IsNullOrWhiteSpace(email))
            {
                _logger.LogWarning("GitHub callback missing email (user may have hidden it or scope is insufficient)");
                await HttpContext.SignOutAsync("External");
                return Redirect($"{clientUrl}/login?error=github_email_required");
            }

            var auth = await _authService.LoginWithGitHubAsync(
                email,
                name ?? email,
                avatarUrl,
                githubId
            );

            // Clean up External Cookie immediately to avoid polluting subsequent logins
            await HttpContext.SignOutAsync("External");

            if (!auth.Succeeded)
            {
                var err = auth.Errors?.FirstOrDefault() ?? "GitHub login failed";
                _logger.LogWarning("GitHub login failed: {Error}", err);
                return Redirect($"{clientUrl}/login?error={Uri.EscapeDataString(err)}");
            }

            return Redirect(
                $"{clientUrl}/auth/callback" +
                $"?token={Uri.EscapeDataString(auth.Token ?? string.Empty)}" +
                $"&userId={auth.UserId}" +
                $"&email={Uri.EscapeDataString(auth.Email ?? string.Empty)}" +
                $"&displayName={Uri.EscapeDataString(auth.DisplayName ?? string.Empty)}"
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception occurred while processing GitHub callback");
            // Try to clean up External Cookie
            try { await HttpContext.SignOutAsync("External"); } catch { /* Ignore */ }
            return Redirect($"{clientUrl}/login?error=github_auth_error");
        }
    }

}
