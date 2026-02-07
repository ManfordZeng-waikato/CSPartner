using System.Net;
using System.Net.Http.Json;
using API.Tests.Helpers;
using Application.Common.Interfaces;
using Application.DTOs.Auth;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;

namespace API.Tests.Account;

public class AccountControllerTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;
    private const string TestPassword = "Test" + "1234" + "!";

    public AccountControllerTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Register_returns_ok_when_success()
    {
        var auth = GetAuthService();
        auth.RegisterResult = new AuthResultDto { Succeeded = true, Email = "user@test.local" };

        var client = _factory.CreateClient();
        var response = await client.PostAsJsonAsync("/api/account/register", new RegisterDto
        {
            Email = "user@test.local",
            Password = TestPassword,
            ConfirmPassword = TestPassword
        });

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<AuthResultDto>();
        result.Should().NotBeNull();
        result!.Succeeded.Should().BeTrue();
    }

    [Fact]
    public async Task Login_returns_generic_error_when_failure()
    {
        var auth = GetAuthService();
        auth.LoginResult = new AuthResultDto
        {
            Succeeded = false,
            Errors = new[] { "SOME_ERROR" }
        };

        var client = _factory.CreateClient();
        var response = await client.PostAsJsonAsync("/api/account/login", new LoginDto
        {
            Email = "user@test.local",
            Password = "badpass"
        });

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<AuthResultDto>();
        result.Should().NotBeNull();
        result!.Errors.Should().ContainSingle().Which.Should().Be("Invalid email or password");
    }

    [Fact]
    public async Task Login_returns_original_result_when_email_not_confirmed()
    {
        var auth = GetAuthService();
        auth.LoginResult = new AuthResultDto
        {
            Succeeded = false,
            Errors = new[] { "EMAIL_NOT_CONFIRMED" }
        };

        var client = _factory.CreateClient();
        var response = await client.PostAsJsonAsync("/api/account/login", new LoginDto
        {
            Email = "user@test.local",
            Password = "badpass"
        });

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<AuthResultDto>();
        result.Should().NotBeNull();
        result!.Errors.Should().Contain("EMAIL_NOT_CONFIRMED");
    }

    [Fact]
    public async Task ResendConfirmationEmail_returns_bad_request_when_failed()
    {
        var auth = GetAuthService();
        auth.ResendResult = (false, "nope");

        var client = _factory.CreateClient();
        var response = await client.GetAsync("/api/account/resendConfirmationEmail?email=user@test.local");

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task ConfirmEmail_returns_bad_request_when_code_missing()
    {
        var client = _factory.CreateClient();
        var response = await client.PostAsync($"/api/account/confirmEmail?userId={Guid.NewGuid()}&code=", content: null);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task ConfirmEmail_returns_bad_request_when_failed()
    {
        var auth = GetAuthService();
        auth.ConfirmEmailResult = new AuthResultDto { Succeeded = false, Errors = new[] { "bad" } };

        var client = _factory.CreateClient();
        var response = await client.PostAsync($"/api/account/confirmEmail?userId={Guid.NewGuid()}&code=code", content: null);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GitHubLogin_returns_redirect()
    {
        var client = _factory.CreateClient(new Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });

        var response = await client.GetAsync("/api/account/github-login");

        response.StatusCode.Should().Be(HttpStatusCode.Redirect);
        response.Headers.Location.Should().NotBeNull();
    }

    [Fact]
    public async Task GitHubCallback_redirects_when_external_auth_missing()
    {
        var client = _factory.CreateClient(new Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });

        var response = await client.GetAsync("/api/account/github-callback");

        response.StatusCode.Should().Be(HttpStatusCode.Redirect);
        response.Headers.Location!.ToString().Should().Contain("/login?error=github_auth_failed");
    }

    [Fact]
    public async Task RequestPasswordReset_returns_bad_request_when_failed()
    {
        var auth = GetAuthService();
        auth.RequestPasswordResetResult = (false, "nope");

        var client = _factory.CreateClient();
        var response = await client.PostAsJsonAsync("/api/account/requestPasswordReset", new RequestPasswordResetDto
        {
            Email = "user@test.local"
        });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task ResetPassword_returns_bad_request_when_failed()
    {
        var auth = GetAuthService();
        auth.ResetPasswordResult = (false, "nope");

        var client = _factory.CreateClient();
        var response = await client.PostAsJsonAsync("/api/account/resetPassword", new ResetPasswordDto
        {
            Email = "user@test.local",
            NewPassword = TestPassword,
            ConfirmPassword = TestPassword,
            Code = "code"
        });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Logout_returns_ok_even_on_exception()
    {
        var auth = GetAuthService();
        auth.ThrowOnLogout = true;

        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Add("X-Test-Auth", "true");

        var response = await client.PostAsync("/api/account/logout", content: null);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    private FakeAuthService GetAuthService()
    {
        using var scope = _factory.Services.CreateScope();
        return (FakeAuthService)scope.ServiceProvider.GetRequiredService<IAuthService>();
    }
}
