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
        
        // Send confirmation email, but don't fail registration if email sending fails
        // User can resend email from check-email page
        try
        {
            await SendConfirmationEmailAsync(user, user.Email);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send confirmation email for user {Email} during registration", user.Email);
            // Continue with registration even if email sending failed
        }

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

        var confirmEmailUrl = $"{_configuration["ClientApp:ClientUrl"]}/confirm-email?userId={user.Id}&code={code}";
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

        // Check password first before checking email confirmation
        var signInResult = await _signInManager.CheckPasswordSignInAsync(
            user, dto.Password, lockoutOnFailure: false);

        if (!signInResult.Succeeded)
        {
            _logger.LogWarning("User {Email} login failed: incorrect password", dto.Email);
            return Failure("The password you entered is incorrect. Please try again.");
        }

        // Check if email is confirmed (explicit check to enforce RequireConfirmedEmail setting)
        if (!user.EmailConfirmed)
        {
            _logger.LogWarning("User {Email} login failed: email not confirmed, sending confirmation email", dto.Email);
            // Send confirmation email automatically
            // Wrap in try-catch to ensure we always return EMAIL_NOT_CONFIRMED response
            // even if email sending fails, so user can still access check-email page
            try
            {
                await SendConfirmationEmailAsync(user, user.Email!);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send confirmation email for user {Email} during login", dto.Email);
                // Continue to return EMAIL_NOT_CONFIRMED response even if email sending failed
            }
            // Return failure with email address so frontend can redirect to check email page
            return new AuthResultDto
            {
                Succeeded = false,
                Email = user.Email,
                Errors = new[] { "EMAIL_NOT_CONFIRMED" }
            };
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

    public async Task<AuthResultDto> ConfirmEmailAsync(Guid userId, string code)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user == null)
        {
            return Failure("Invalid confirmation link. User not found.");
        }

        if (user.EmailConfirmed)
        {
            // Email already confirmed, return success with token for auto-login
            _logger.LogInformation("Email already confirmed for user {Email}, returning token", user.Email);
            var profile = await _context.UserProfiles
                .FirstOrDefaultAsync(p => p.UserId == user.Id);
            var displayName = profile?.DisplayName ?? user.Email;
            var roles = await _userManager.GetRolesAsync(user);
            return Success(user, displayName, roles);
        }

        try
        {
            // Decode the base64 URL encoded code
            var decodedCode = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(code));
            
            var result = await _userManager.ConfirmEmailAsync(user, decodedCode);
            if (result.Succeeded)
            {
                _logger.LogInformation("Email confirmed for user {Email}", user.Email);
                
                // Reload user to get the latest EmailConfirmed status
                var reloadedUser = await _userManager.FindByIdAsync(userId.ToString());
                if (reloadedUser == null)
                {
                    _logger.LogError("Failed to reload user {UserId} after email confirmation", userId);
                    return Failure("Email confirmed but failed to complete login. Please try logging in.");
                }

                // Get user profile and roles
                var profile = await _context.UserProfiles
                    .FirstOrDefaultAsync(p => p.UserId == reloadedUser.Id);
                var displayName = profile?.DisplayName ?? reloadedUser.Email;
                var roles = await _userManager.GetRolesAsync(reloadedUser);
                
                // Return success with token for auto-login
                return Success(reloadedUser, displayName, roles);
            }
            else
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                _logger.LogWarning("Email confirmation failed for user {Email}: {Errors}", user.Email, errors);
                return Failure($"Email confirmation failed: {errors}");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to confirm email for user {UserId}", userId);
            return Failure("Invalid or expired confirmation link. Please request a new one.");
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

    public async Task<(bool Succeeded, string Message)> RequestPasswordResetAsync(RequestPasswordResetDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Email))
        {
            return (false, "Email is required");
        }

        var user = await _userManager.FindByEmailAsync(dto.Email);
        if (user == null)
        {
            // Tell user that email is not registered
            _logger.LogWarning("Password reset requested for non-existent email: {Email}", dto.Email);
            return (false, "This email address is not registered. Please check your email or sign up for a new account.");
        }

        if (!user.EmailConfirmed)
        {
            // Tell user that email is not confirmed
            _logger.LogWarning("Password reset requested for unconfirmed email: {Email}", dto.Email);
            return (false, "This email address has not been confirmed. Please confirm your email first before resetting your password.");
        }

        try
        {
            // Generate password reset token
            var resetToken = await _userManager.GeneratePasswordResetTokenAsync(user);
            // Encode the token for URL safety
            var encodedToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(resetToken));

            // Send password reset email
            await _emailService.SendPasswordResetCodeAsync(
                user.Email!,
                user.UserName ?? user.Email!,
                encodedToken);

            _logger.LogInformation("Password reset email sent for user {Email}", dto.Email);
            return (true, "A password reset link has been sent to your email. Please check your inbox.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send password reset email for user {Email}", dto.Email);
            return (false, "Failed to send password reset email. Please try again later.");
        }
    }

    public async Task<(bool Succeeded, string Message)> ResetPasswordAsync(ResetPasswordDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Email))
        {
            return (false, "Email is required");
        }

        if (string.IsNullOrWhiteSpace(dto.Code))
        {
            return (false, "Reset code is required");
        }

        if (dto.NewPassword != dto.ConfirmPassword)
        {
            return (false, "Passwords do not match");
        }

        var user = await _userManager.FindByEmailAsync(dto.Email);
        if (user == null)
        {
            // Don't reveal if email exists or not for security reasons
            _logger.LogWarning("Password reset attempted for non-existent email: {Email}", dto.Email);
            return (false, "Invalid reset code or email address.");
        }

        try
        {
            // Decode the base64 URL encoded token
            var decodedToken = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(dto.Code));

            // Reset the password
            var result = await _userManager.ResetPasswordAsync(user, decodedToken, dto.NewPassword);

            if (result.Succeeded)
            {
                _logger.LogInformation("Password reset successful for user {Email}", dto.Email);
                return (true, "Password has been reset successfully. You can now login with your new password.");
            }
            else
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                _logger.LogWarning("Password reset failed for user {Email}: {Errors}", dto.Email, errors);
                return (false, $"Password reset failed: {errors}");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to reset password for user {Email}", dto.Email);
            return (false, "Invalid or expired reset code. Please request a new password reset.");
        }
    }

    public async Task<AuthResultDto> LoginWithGitHubAsync(string email, string name, string? avatarUrl, string githubId)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            return Failure("Email is required for GitHub login");
        }

        if (string.IsNullOrWhiteSpace(githubId))
        {
            return Failure("GitHub ID is required");
        }

        // Try to find existing user by email
        var user = await _userManager.FindByEmailAsync(email);
        
        if (user == null)
        {
            // Create new user for GitHub login
            user = new ApplicationUser
            {
                UserName = email,
                Email = email,
                EmailConfirmed = true // GitHub email is already verified
            };

            var createResult = await _userManager.CreateAsync(user);
            if (!createResult.Succeeded)
            {
                _logger.LogError("Failed to create user for GitHub login: {Errors}", 
                    string.Join(", ", createResult.Errors.Select(e => e.Description)));
                return Failure(createResult.Errors.Select(e => e.Description));
            }

            // Add user to User role
            if (await _roleManager.RoleExistsAsync("User"))
            {
                await _userManager.AddToRoleAsync(user, "User");
            }

            // Create user profile
            var displayName = string.IsNullOrWhiteSpace(name) ? email : name.Trim();
            var profile = new UserProfile(user.Id);
            profile.Update(displayName, null, avatarUrl, null, null);

            await _context.UserProfiles.AddAsync(profile);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Created new user {Email} via GitHub OAuth", email);
        }
        else
        {
            // User exists, update profile if needed
            var profile = await _context.UserProfiles
                .FirstOrDefaultAsync(p => p.UserId == user.Id);

            if (profile == null)
            {
                // Create profile if it doesn't exist
                var displayName = string.IsNullOrWhiteSpace(name) ? email : name.Trim();
                profile = new UserProfile(user.Id);
                profile.Update(displayName, null, avatarUrl, null, null);
                await _context.UserProfiles.AddAsync(profile);
            }
            else
            {
                // Update existing profile with GitHub info if not already set
                var displayName = string.IsNullOrWhiteSpace(name) ? email : name.Trim();
                if (string.IsNullOrWhiteSpace(profile.DisplayName))
                {
                    profile.Update(displayName, profile.Bio, avatarUrl ?? profile.AvatarUrl, 
                        profile.SteamProfileUrl, profile.FaceitProfileUrl);
                }
                else if (!string.IsNullOrWhiteSpace(avatarUrl) && string.IsNullOrWhiteSpace(profile.AvatarUrl))
                {
                    profile.Update(profile.DisplayName, profile.Bio, avatarUrl, 
                        profile.SteamProfileUrl, profile.FaceitProfileUrl);
                }
            }

            // Ensure email is confirmed for existing users logging in via GitHub
            if (!user.EmailConfirmed)
            {
                user.EmailConfirmed = true;
                await _userManager.UpdateAsync(user);
            }

            await _context.SaveChangesAsync();
            _logger.LogInformation("User {Email} logged in via GitHub OAuth", email);
        }

        // Associate GitHub login with user account (for future reference)
        try
        {
            var existingLogins = await _userManager.GetLoginsAsync(user);
            var githubLogin = existingLogins.FirstOrDefault(l => l.LoginProvider == "GitHub");
            
            if (githubLogin == null)
            {
                await _userManager.AddLoginAsync(user, new UserLoginInfo("GitHub", githubId, "GitHub"));
            }
        }
        catch (Exception ex)
        {
            // Log but don't fail - login association is optional
            _logger.LogWarning(ex, "Failed to associate GitHub login with user {Email}", email);
        }

        // Get user profile and roles
        var finalProfile = await _context.UserProfiles
            .FirstOrDefaultAsync(p => p.UserId == user.Id);
        var displayNameFinal = finalProfile?.DisplayName ?? name ?? email;
        var roles = await _userManager.GetRolesAsync(user);

        return Success(user, displayNameFinal, roles);
    }
}

