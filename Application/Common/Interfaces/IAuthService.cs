using Application.DTOs.Auth;

namespace Application.Common.Interfaces;

public interface IAuthService
{
    Task<AuthResultDto> RegisterAsync(RegisterDto dto);
    Task<AuthResultDto> LoginAsync(LoginDto dto);
    Task LogoutAsync();
    Task<(bool Succeeded, string Message)> ResendConfirmationEmailAsync(string email);
    Task<AuthResultDto> ConfirmEmailAsync(Guid userId, string code);
    Task<(bool Succeeded, string Message)> RequestPasswordResetAsync(RequestPasswordResetDto dto);
    Task<(bool Succeeded, string Message)> ResetPasswordAsync(ResetPasswordDto dto);
    Task<AuthResultDto> LoginWithGitHubAsync(string email, string name, string? avatarUrl, string githubId);
}

