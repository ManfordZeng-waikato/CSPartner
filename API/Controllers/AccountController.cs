using Application.DTOs.Auth;
using Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

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
        if (!result.Succeeded)
        {
            return Unauthorized(new { errors = result.Errors });
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
}

