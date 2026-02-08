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
    private static string CreateStrongPassword()
    {
        return $"Test{Guid.NewGuid():N}!A1";
    }

    [Fact]
    public async Task RegisterAsync_returns_failure_when_email_exists()
    {
        using var scope = AuthServiceTestScope.Create();

        var userManager = scope.Provider.GetRequiredService<UserManager<ApplicationUser>>();
        var existing = new ApplicationUser { UserName = "user@test.local", Email = "user@test.local" };
        var existingPassword = CreateStrongPassword();
        await userManager.CreateAsync(existing, existingPassword);

        var authService = scope.Provider.GetRequiredService<AuthService>();
        var password = CreateStrongPassword();

        var result = await authService.RegisterAsync(new RegisterDto
        {
            Email = "user@test.local",
            Password = password,
            ConfirmPassword = password
        });

        result.Succeeded.Should().BeFalse();
        result.Errors.Should().Contain("Email is already registered");
    }

    [Fact]
    public async Task LoginAsync_returns_generic_error_when_user_missing()
    {
        using var scope = AuthServiceTestScope.Create();

        var authService = scope.Provider.GetRequiredService<AuthService>();
        var password = CreateStrongPassword();

        var result = await authService.LoginAsync(new LoginDto
        {
            Email = "missing@test.local",
            Password = password
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

    [Fact]
    public async Task RegisterAsync_returns_pending_confirmation_when_email_not_confirmed()
    {
        using var scope = AuthServiceTestScope.Create();

        var authService = scope.Provider.GetRequiredService<AuthService>();
        var password = CreateStrongPassword();

        var result = await authService.RegisterAsync(new RegisterDto
        {
            Email = $"user-{Guid.NewGuid():N}@test.local",
            Password = password,
            ConfirmPassword = password
        });

        result.Succeeded.Should().BeTrue();
        result.Token.Should().BeNull();
        result.Errors.Should().ContainSingle()
            .Which.Should().Contain("Registration successful");
    }

    [Fact]
    public async Task LoginAsync_returns_email_not_confirmed_when_unconfirmed_user_logs_in()
    {
        using var scope = AuthServiceTestScope.Create();

        var userManager = scope.Provider.GetRequiredService<UserManager<ApplicationUser>>();
        var password = CreateStrongPassword();
        var user = new ApplicationUser { UserName = "unconfirmed@test.local", Email = "unconfirmed@test.local" };
        await userManager.CreateAsync(user, password);

        var authService = scope.Provider.GetRequiredService<AuthService>();
        var result = await authService.LoginAsync(new LoginDto
        {
            Email = "unconfirmed@test.local",
            Password = password
        });

        result.Succeeded.Should().BeFalse();
        result.Errors.Should().Contain("EMAIL_NOT_CONFIRMED");
        result.Email.Should().Be("unconfirmed@test.local");
    }

    [Fact]
    public async Task ConfirmEmailAsync_returns_failure_when_user_missing()
    {
        using var scope = AuthServiceTestScope.Create();

        var authService = scope.Provider.GetRequiredService<AuthService>();

        var result = await authService.ConfirmEmailAsync(Guid.NewGuid(), "code");

        result.Succeeded.Should().BeFalse();
        result.Errors.Should().Contain("Invalid confirmation link. User not found.");
    }

    [Fact]
    public async Task ResendConfirmationEmailAsync_returns_error_when_email_empty()
    {
        using var scope = AuthServiceTestScope.Create();

        var authService = scope.Provider.GetRequiredService<AuthService>();

        var result = await authService.ResendConfirmationEmailAsync(string.Empty);

        result.Succeeded.Should().BeFalse();
        result.Message.Should().Be("Email is required");
    }

    [Fact]
    public async Task ResendConfirmationEmailAsync_returns_success_when_user_missing()
    {
        using var scope = AuthServiceTestScope.Create();

        var authService = scope.Provider.GetRequiredService<AuthService>();

        var result = await authService.ResendConfirmationEmailAsync("missing@test.local");

        result.Succeeded.Should().BeTrue();
        result.Message.Should().Contain("confirmation link has been sent");
    }

    [Fact]
    public async Task ResendConfirmationEmailAsync_returns_error_when_email_confirmed()
    {
        using var scope = AuthServiceTestScope.Create();

        var userManager = scope.Provider.GetRequiredService<UserManager<ApplicationUser>>();
        var password = CreateStrongPassword();
        var user = new ApplicationUser { UserName = "confirmed@test.local", Email = "confirmed@test.local", EmailConfirmed = true };
        await userManager.CreateAsync(user, password);

        var authService = scope.Provider.GetRequiredService<AuthService>();

        var result = await authService.ResendConfirmationEmailAsync("confirmed@test.local");

        result.Succeeded.Should().BeFalse();
        result.Message.Should().Be("Email is already confirmed");
    }

    [Fact]
    public async Task RequestPasswordResetAsync_returns_error_when_email_unconfirmed()
    {
        using var scope = AuthServiceTestScope.Create();

        var userManager = scope.Provider.GetRequiredService<UserManager<ApplicationUser>>();
        var password = CreateStrongPassword();
        var user = new ApplicationUser { UserName = "unconfirmed2@test.local", Email = "unconfirmed2@test.local" };
        await userManager.CreateAsync(user, password);

        var authService = scope.Provider.GetRequiredService<AuthService>();

        var result = await authService.RequestPasswordResetAsync(new RequestPasswordResetDto
        {
            Email = "unconfirmed2@test.local"
        });

        result.Succeeded.Should().BeFalse();
        result.Message.Should().Contain("not been confirmed");
    }

    [Fact]
    public async Task RequestPasswordResetAsync_returns_error_when_email_missing()
    {
        using var scope = AuthServiceTestScope.Create();

        var authService = scope.Provider.GetRequiredService<AuthService>();

        var result = await authService.RequestPasswordResetAsync(new RequestPasswordResetDto
        {
            Email = " "
        });

        result.Succeeded.Should().BeFalse();
        result.Message.Should().Be("Email is required");
    }

    [Fact]
    public async Task ResetPasswordAsync_returns_error_when_passwords_mismatch()
    {
        using var scope = AuthServiceTestScope.Create();

        var authService = scope.Provider.GetRequiredService<AuthService>();
        var password = CreateStrongPassword();

        var result = await authService.ResetPasswordAsync(new ResetPasswordDto
        {
            Email = "user@test.local",
            NewPassword = password,
            ConfirmPassword = password + "x",
            Code = "code"
        });

        result.Succeeded.Should().BeFalse();
        result.Message.Should().Be("Passwords do not match");
    }

    [Fact]
    public async Task ResetPasswordAsync_returns_error_when_code_missing()
    {
        using var scope = AuthServiceTestScope.Create();

        var authService = scope.Provider.GetRequiredService<AuthService>();
        var password = CreateStrongPassword();

        var result = await authService.ResetPasswordAsync(new ResetPasswordDto
        {
            Email = "user@test.local",
            NewPassword = password,
            ConfirmPassword = password,
            Code = ""
        });

        result.Succeeded.Should().BeFalse();
        result.Message.Should().Be("Reset code is required");
    }

    [Fact]
    public async Task ConfirmEmailAsync_returns_success_when_already_confirmed()
    {
        using var scope = AuthServiceTestScope.Create();

        var userManager = scope.Provider.GetRequiredService<UserManager<ApplicationUser>>();
        var password = CreateStrongPassword();
        var user = new ApplicationUser { UserName = "confirmed2@test.local", Email = "confirmed2@test.local", EmailConfirmed = true };
        await userManager.CreateAsync(user, password);

        var authService = scope.Provider.GetRequiredService<AuthService>();

        var result = await authService.ConfirmEmailAsync(user.Id, "code");

        result.Succeeded.Should().BeTrue();
        result.Token.Should().NotBeNullOrWhiteSpace();
        result.Email.Should().Be("confirmed2@test.local");
    }

    [Fact]
    public async Task ConfirmEmailAsync_returns_failure_when_code_invalid()
    {
        using var scope = AuthServiceTestScope.Create();

        var userManager = scope.Provider.GetRequiredService<UserManager<ApplicationUser>>();
        var password = CreateStrongPassword();
        var user = new ApplicationUser { UserName = "pending@test.local", Email = "pending@test.local" };
        await userManager.CreateAsync(user, password);

        var authService = scope.Provider.GetRequiredService<AuthService>();

        var result = await authService.ConfirmEmailAsync(user.Id, "not-base64");

        result.Succeeded.Should().BeFalse();
        result.Errors.Should().ContainSingle()
            .Which.Should().Contain("Invalid or expired confirmation link");
    }

    [Fact]
    public async Task ResetPasswordAsync_returns_error_when_user_missing()
    {
        using var scope = AuthServiceTestScope.Create();

        var authService = scope.Provider.GetRequiredService<AuthService>();
        var password = CreateStrongPassword();

        var result = await authService.ResetPasswordAsync(new ResetPasswordDto
        {
            Email = "missing@test.local",
            NewPassword = password,
            ConfirmPassword = password,
            Code = "code"
        });

        result.Succeeded.Should().BeFalse();
        result.Message.Should().Be("Invalid reset code or email address.");
    }

    [Fact]
    public async Task LoginWithGitHubAsync_returns_failure_when_github_id_missing()
    {
        using var scope = AuthServiceTestScope.Create();

        var authService = scope.Provider.GetRequiredService<AuthService>();

        var result = await authService.LoginWithGitHubAsync("user@test.local", "User", null, "");

        result.Succeeded.Should().BeFalse();
        result.Errors.Should().Contain("GitHub ID is required");
    }

    [Fact]
    public async Task LoginWithGitHubAsync_creates_user_and_profile_when_missing()
    {
        using var scope = AuthServiceTestScope.Create();

        var roleManager = scope.Provider.GetRequiredService<RoleManager<ApplicationRole>>();
        await roleManager.CreateAsync(new ApplicationRole { Name = "User" });

        var authService = scope.Provider.GetRequiredService<AuthService>();
        var userManager = scope.Provider.GetRequiredService<UserManager<ApplicationUser>>();

        var result = await authService.LoginWithGitHubAsync(
            "github@test.local",
            "GitHub User",
            "https://avatar.test/img.png",
            "github-123");

        result.Succeeded.Should().BeTrue();
        result.Email.Should().Be("github@test.local");
        result.Token.Should().NotBeNullOrWhiteSpace();

        var user = await userManager.FindByEmailAsync("github@test.local");
        user.Should().NotBeNull();
        user!.EmailConfirmed.Should().BeTrue();

        var roles = await userManager.GetRolesAsync(user);
        roles.Should().Contain("User");
    }
}
