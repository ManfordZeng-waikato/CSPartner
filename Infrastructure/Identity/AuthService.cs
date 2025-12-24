using System.Linq;
using System.Text;
using Application.Common.Interfaces;
using Application.DTOs.Auth;
using Application.Interfaces.Services;
using Domain.Users;
using Infrastructure.Persistence.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Identity;

public class AuthService : IAuthService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly RoleManager<ApplicationRole> _roleManager;
    private readonly IApplicationDbContext _context;
    private readonly IJwtService _jwtService;
    private readonly ILogger<AuthService> _logger;
    private readonly IEmailService _emailService;
    private readonly IConfiguration _configuration;
    public AuthService(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        RoleManager<ApplicationRole> roleManager,
        IApplicationDbContext context,
        IJwtService jwtService,
        ILogger<AuthService> logger,
        IEmailService emailService,
        IConfiguration configuration)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _roleManager = roleManager;
        _context = context;
        _jwtService = jwtService;
        _logger = logger;
        _emailService = emailService;
        _configuration = configuration;
    }

    public async Task<AuthResultDto> RegisterAsync(RegisterDto dto)
    {
        var existingUser = await _userManager.FindByEmailAsync(dto.Email);
        if (existingUser != null)
        {
            return Failure("Email is already registered");
        }

        var user = new ApplicationUser
        {
            UserName = dto.Email,
            Email = dto.Email,
            EmailConfirmed = false  // Require email confirmation before allowing login
        };

        var createResult = await _userManager.CreateAsync(user, dto.Password);
        if (!createResult.Succeeded)
        {
            return Failure(createResult.Errors.Select(e => e.Description));
        }
        await SendConfirmationEmailAsync(user, user.Email);

        if (await _roleManager.RoleExistsAsync("User"))
        {
            await _userManager.AddToRoleAsync(user, "User");
        }

        var displayName = string.IsNullOrWhiteSpace(dto.DisplayName)
            ? dto.Email
            : dto.DisplayName.Trim();

        var profile = new UserProfile(user.Id);
        profile.Update(displayName, null, dto.AvatarUrl, null, null);

        _logger.LogInformation("User {Email} registered successfully, Avatar URL: {AvatarUrl}", user.Email, profile.AvatarUrl ?? "Not set");

        await _context.UserProfiles.AddAsync(profile);
        await _context.SaveChangesAsync();

        var roles = await _userManager.GetRolesAsync(user);

        // Reload user from database to ensure we have the latest EmailConfirmed value
        // This ensures consistency even if CreateAsync modified the user object
        var reloadedUser = await _userManager.FindByIdAsync(user.Id.ToString());
        if (reloadedUser == null)
        {
            _logger.LogError("Failed to reload user {Email} after creation", user.Email);
            return Failure("Failed to complete registration. Please try again.");
        }

        // If email is not confirmed, don't return token - user must confirm email first
        if (!reloadedUser.EmailConfirmed)
        {
            return RegistrationPendingEmailConfirmation(reloadedUser, displayName);
        }

        return Success(reloadedUser, displayName, roles);
    }

    private async Task SendConfirmationEmailAsync(ApplicationUser user, string email)
    {
        var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
        code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));

        var confirmEmailUrl = $"{_configuration["ClientAppUrl:ClientUrl"]}/confirm-email?userId={user.Id}&code={code}";
        await _emailService.SendConfirmationLinkAsync(email, user.UserName ?? email, confirmEmailUrl);
    }

    public async Task<AuthResultDto> LoginAsync(LoginDto dto)
    {
        var user = await _userManager.FindByEmailAsync(dto.Email);
        if (user is null)
        {
            _logger.LogWarning("User {Email} login failed: user does not exist", dto.Email);
            return Failure("The email address you entered does not exist. Please check your email and try again.");
        }

        // Check if email is confirmed (explicit check to enforce RequireConfirmedEmail setting)
        if (!user.EmailConfirmed)
        {
            _logger.LogWarning("User {Email} login failed: email not confirmed", dto.Email);
            return Failure("Your email address has not been confirmed. Please check your email and confirm your account before logging in.");
        }

        var signInResult = await _signInManager.CheckPasswordSignInAsync(
            user, dto.Password, lockoutOnFailure: false);

        if (!signInResult.Succeeded)
        {
            _logger.LogWarning("User {Email} login failed: incorrect password", dto.Email);
            return Failure("The password you entered is incorrect. Please try again.");
        }

        var profile = await _context.UserProfiles
            .FirstOrDefaultAsync(p => p.UserId == user.Id);
        var displayName = profile?.DisplayName ?? user.Email;
        var roles = await _userManager.GetRolesAsync(user);

        _logger.LogInformation("User {Email} logged in successfully", user.Email);
        return Success(user, displayName, roles);
    }

    public async Task LogoutAsync()
    {
        // JWT tokens are stateless, so logout is handled client-side by removing the token
        // This method is kept for API compatibility
        _logger.LogInformation("User logged out (token should be removed client-side)");
        await Task.CompletedTask;
    }

    public async Task<(bool Succeeded, string Message)> ResendConfirmationEmailAsync(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            return (false, "Email is required");
        }

        var user = await _userManager.FindByEmailAsync(email);
        if (user == null)
        {
            // Don't reveal if email exists or not for security reasons
            return (true, "If the email exists, a confirmation link has been sent.");
        }

        if (user.EmailConfirmed)
        {
            return (false, "Email is already confirmed");
        }

        try
        {
            await SendConfirmationEmailAsync(user, user.Email!);
            _logger.LogInformation("Confirmation email resent for user {Email}", email);
            return (true, "Confirmation email has been sent. Please check your inbox.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to resend confirmation email for user {Email}", email);
            return (false, "Failed to send confirmation email. Please try again later.");
        }
    }

    private AuthResultDto Success(ApplicationUser user, string? displayName, IEnumerable<string> roles)
    {
        var token = _jwtService.GenerateToken(user.Id, user.Email ?? string.Empty, roles);
        return new()
        {
            Succeeded = true,
            UserId = user.Id,
            Email = user.Email,
            DisplayName = displayName,
            Token = token
        };
    }

    private static AuthResultDto Failure(params string[] errors)
        => new()
        {
            Succeeded = false,
            Errors = errors
        };

    private static AuthResultDto Failure(IEnumerable<string> errors)
        => new()
        {
            Succeeded = false,
            Errors = errors.ToArray()
        };

    private static AuthResultDto RegistrationPendingEmailConfirmation(ApplicationUser user, string? displayName)
        => new()
        {
            Succeeded = true,
            UserId = user.Id,
            Email = user.Email,
            DisplayName = displayName,
            Token = null,  // No token until email is confirmed
            Errors = new[] { "Registration successful. Please check your email to confirm your account before logging in." }
        };
}

