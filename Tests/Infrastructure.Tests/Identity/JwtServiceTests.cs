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
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:SecretKey"] = "TestSecretKey-For-Integration-Only-1234567890",
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
}
