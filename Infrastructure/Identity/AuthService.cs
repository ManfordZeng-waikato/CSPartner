using System.Linq;
using Application.DTOs.Auth;
using Application.Interfaces;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Domain.Users;
using Infrastructure.Persistence.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Identity;

public class AuthService : IAuthService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly RoleManager<ApplicationRole> _roleManager;
    private readonly IUserProfileRepository _profileRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IJwtService _jwtService;
    private readonly ILogger<AuthService> _logger;

    public AuthService(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        RoleManager<ApplicationRole> roleManager,
        IUserProfileRepository profileRepository,
        IUnitOfWork unitOfWork,
        IJwtService jwtService,
        ILogger<AuthService> logger)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _roleManager = roleManager;
        _profileRepository = profileRepository;
        _unitOfWork = unitOfWork;
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
        profile.Update(displayName, null, null, null, null);

        await _profileRepository.AddAsync(profile);
        await _unitOfWork.SaveChangesAsync();

        var roles = await _userManager.GetRolesAsync(user);

        _logger.LogInformation("用户 {Email} 注册成功", user.Email);
        return Success(user, displayName, roles);
    }

    public async Task<AuthResultDto> LoginAsync(LoginDto dto)
    {
        var user = await _userManager.FindByEmailAsync(dto.Email);
        if (user is null)
        {
            _logger.LogWarning("用户 {Email} 登录失败：用户不存在", dto.Email);
            return Failure("Incorrect email or password");
        }

        var signInResult = await _signInManager.CheckPasswordSignInAsync(
            user, dto.Password, lockoutOnFailure: false);

        if (!signInResult.Succeeded)
        {
            _logger.LogWarning("用户 {Email} 登录失败：密码错误", dto.Email);
            return Failure("Incorrect email or password");
        }

        var profile = await _profileRepository.GetByUserIdAsync(user.Id);
        var displayName = profile?.DisplayName ?? user.Email;
        var roles = await _userManager.GetRolesAsync(user);

        _logger.LogInformation("用户 {Email} 登录成功", user.Email);
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

