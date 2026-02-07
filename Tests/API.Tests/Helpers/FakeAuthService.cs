using Application.Common.Interfaces;
using Application.DTOs.Auth;
using Domain.Exceptions;

namespace API.Tests.Helpers;

public class FakeAuthService : IAuthService
{
    public AuthResultDto RegisterResult { get; set; } = new() { Succeeded = true };
    public AuthResultDto LoginResult { get; set; } = new() { Succeeded = true, Token = "test-token" };
    public AuthResultDto ConfirmEmailResult { get; set; } = new() { Succeeded = true };
    public AuthResultDto LoginWithGitHubResult { get; set; } = new() { Succeeded = true, Token = "github-token" };
    public (bool Succeeded, string Message) ResendResult { get; set; } = (true, "ok");
    public (bool Succeeded, string Message) RequestPasswordResetResult { get; set; } = (true, "ok");
    public (bool Succeeded, string Message) ResetPasswordResult { get; set; } = (true, "ok");
    public bool ThrowOnLogout { get; set; }

    public Task<AuthResultDto> RegisterAsync(RegisterDto dto) => Task.FromResult(RegisterResult);

    public Task<AuthResultDto> LoginAsync(LoginDto dto) => Task.FromResult(LoginResult);

    public Task LogoutAsync()
    {
        if (ThrowOnLogout)
        {
            throw AuthenticationRequiredException.ForOperation("logout");
        }
        return Task.CompletedTask;
    }

    public Task<(bool Succeeded, string Message)> ResendConfirmationEmailAsync(string email)
        => Task.FromResult(ResendResult);

    public Task<AuthResultDto> ConfirmEmailAsync(Guid userId, string code)
        => Task.FromResult(ConfirmEmailResult);

    public Task<(bool Succeeded, string Message)> RequestPasswordResetAsync(RequestPasswordResetDto dto)
        => Task.FromResult(RequestPasswordResetResult);

    public Task<(bool Succeeded, string Message)> ResetPasswordAsync(ResetPasswordDto dto)
        => Task.FromResult(ResetPasswordResult);

    public Task<AuthResultDto> LoginWithGitHubAsync(string email, string name, string? avatarUrl, string githubId)
        => Task.FromResult(LoginWithGitHubResult);
}
