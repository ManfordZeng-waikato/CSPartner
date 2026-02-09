using FluentAssertions;
using Infrastructure.Identity;
using Microsoft.Extensions.Configuration;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Infrastructure.Tests.Identity;

public class JwtServiceTests
{
    [Fact]
    public void GenerateToken_throws_when_secret_missing()
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:Issuer"] = "issuer",
                ["Jwt:Audience"] = "audience"
            })
            .Build();

        var service = new JwtService(config);

        var act = () => service.GenerateToken(Guid.NewGuid(), "user@test.local", Array.Empty<string>());

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void GenerateToken_includes_claims_and_roles()
    {
        var secret = Guid.NewGuid().ToString("N") + Guid.NewGuid().ToString("N");
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:SecretKey"] = secret,
                ["Jwt:Issuer"] = "issuer",
                ["Jwt:Audience"] = "audience",
                ["Jwt:ExpirationMinutes"] = "60"
            })
            .Build();

        var service = new JwtService(config);
        var userId = Guid.NewGuid();

        var token = service.GenerateToken(userId, "user@test.local", new[] { "User", "Admin" });

        var handler = new JwtSecurityTokenHandler();
        var jwt = handler.ReadJwtToken(token);

        jwt.Claims.Should().Contain(c => c.Type == JwtRegisteredClaimNames.Sub && c.Value == userId.ToString());
        jwt.Claims.Should().Contain(c => c.Type == JwtRegisteredClaimNames.Email && c.Value == "user@test.local");
        jwt.Claims.Should().Contain(c => c.Type == ClaimTypes.Role && c.Value == "User");
        jwt.Claims.Should().Contain(c => c.Type == ClaimTypes.Role && c.Value == "Admin");
    }

    [Fact]
    public void GenerateToken_throws_when_issuer_missing()
    {
        var secret = Guid.NewGuid().ToString("N") + Guid.NewGuid().ToString("N");
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:SecretKey"] = secret,
                ["Jwt:Audience"] = "audience"
            })
            .Build();

        var service = new JwtService(config);

        var act = () => service.GenerateToken(Guid.NewGuid(), "user@test.local", Array.Empty<string>());

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void GenerateToken_throws_when_audience_missing()
    {
        var secret = Guid.NewGuid().ToString("N") + Guid.NewGuid().ToString("N");
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:SecretKey"] = secret,
                ["Jwt:Issuer"] = "issuer"
            })
            .Build();

        var service = new JwtService(config);

        var act = () => service.GenerateToken(Guid.NewGuid(), "user@test.local", Array.Empty<string>());

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void GenerateToken_uses_default_expiration_minutes()
    {
        var secret = Guid.NewGuid().ToString("N") + Guid.NewGuid().ToString("N");
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:SecretKey"] = secret,
                ["Jwt:Issuer"] = "issuer",
                ["Jwt:Audience"] = "audience"
            })
            .Build();

        var service = new JwtService(config);

        var token = service.GenerateToken(Guid.NewGuid(), "user@test.local", Array.Empty<string>());

        var handler = new JwtSecurityTokenHandler();
        var jwt = handler.ReadJwtToken(token);
        var remainingMinutes = (jwt.ValidTo - DateTime.UtcNow).TotalMinutes;

        remainingMinutes.Should().BeGreaterThan(1430);
        remainingMinutes.Should().BeLessThan(1450);
    }
}
