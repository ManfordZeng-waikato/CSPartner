using System.Linq;
using Application.Common.Interfaces;
using Application.DTOs.Auth;
using Application.Interfaces.Services;
using Domain.Users;
using Infrastructure.Persistence.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
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

    public AuthService(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        RoleManager<ApplicationRole> roleManager,
        IApplicationDbContext context,
        IJwtService jwtService,
        ILogger<AuthService> logger)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _roleManager = roleManager;
        _context = context;
        _jwtService = jwtService;
        _logger = logger;
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
            EmailConfirmed = true
        };

        var createResult = await _userManager.CreateAsync(user, dto.Password);
        if (!createResult.Succeeded)
        {
            return Failure(createResult.Errors.Select(e => e.Description));
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

        _logger.LogInformation("User {Email} registered successfully", user.Email);
        return Success(user, displayName, roles);
    }

    public async Task<AuthResultDto> LoginAsync(LoginDto dto)
    {
        var user = await _userManager.FindByEmailAsync(dto.Email);
        if (user is null)
        {
            _logger.LogWarning("User {Email} login failed: user does not exist", dto.Email);
            return Failure("The email address you entered does not exist. Please check your email and try again.");
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
}

