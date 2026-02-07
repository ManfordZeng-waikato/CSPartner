using Application.DTOs.Auth;
using FluentAssertions;
using Infrastructure.Identity;
using Infrastructure.Persistence.Identity;
using Infrastructure.Tests.Helpers;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.Tests.Identity;

public class AuthServiceTests
{
    [Fact]
    public async Task RegisterAsync_returns_failure_when_email_exists()
    {
        using var scope = AuthServiceTestScope.Create();

        var userManager = scope.Provider.GetRequiredService<UserManager<ApplicationUser>>();
        var existing = new ApplicationUser { UserName = "user@test.local", Email = "user@test.local" };
        await userManager.CreateAsync(existing, "Test1234!");

        var authService = scope.Provider.GetRequiredService<AuthService>();

        var result = await authService.RegisterAsync(new RegisterDto
        {
            Email = "user@test.local",
            Password = "Test1234!",
            ConfirmPassword = "Test1234!"
        });

        result.Succeeded.Should().BeFalse();
        result.Errors.Should().Contain("Email is already registered");
    }

    [Fact]
    public async Task LoginAsync_returns_generic_error_when_user_missing()
    {
        using var scope = AuthServiceTestScope.Create();

        var authService = scope.Provider.GetRequiredService<AuthService>();

        var result = await authService.LoginAsync(new LoginDto
        {
            Email = "missing@test.local",
            Password = "Test1234!"
        });

        result.Succeeded.Should().BeFalse();
        result.Errors.Should().ContainSingle("Invalid email or password");
    }

    [Fact]
    public async Task LogoutAsync_throws_when_not_authenticated()
    {
        using var scope = AuthServiceTestScope.Create();

        var currentUser = (FakeCurrentUserService)scope.Provider.GetRequiredService<Application.Common.Interfaces.ICurrentUserService>();
        currentUser.UserId = null;

        var authService = scope.Provider.GetRequiredService<AuthService>();

        var act = async () => await authService.LogoutAsync();

        await act.Should().ThrowAsync<Domain.Exceptions.AuthenticationRequiredException>();
    }
}
