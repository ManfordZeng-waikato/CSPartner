using System.Security.Claims;
using FluentAssertions;
using Infrastructure.Identity;
using Microsoft.AspNetCore.Http;

namespace Infrastructure.Tests.Identity;

public class CurrentUserServiceTests
{
    [Fact]
    public void UserId_returns_null_when_not_authenticated()
    {
        var context = new DefaultHttpContext();
        var accessor = new HttpContextAccessor { HttpContext = context };
        var service = new CurrentUserService(accessor);

        service.UserId.Should().BeNull();
    }

    [Fact]
    public void UserId_reads_sub_claim()
    {
        var userId = Guid.NewGuid();
        var identity = new ClaimsIdentity(new[]
        {
            new Claim("sub", userId.ToString())
        }, "test");
        var context = new DefaultHttpContext { User = new ClaimsPrincipal(identity) };

        var accessor = new HttpContextAccessor { HttpContext = context };
        var service = new CurrentUserService(accessor);

        service.UserId.Should().Be(userId);
    }

    [Fact]
    public void UserId_reads_name_identifier()
    {
        var userId = Guid.NewGuid();
        var identity = new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, userId.ToString())
        }, "test");
        var context = new DefaultHttpContext { User = new ClaimsPrincipal(identity) };

        var accessor = new HttpContextAccessor { HttpContext = context };
        var service = new CurrentUserService(accessor);

        service.UserId.Should().Be(userId);
    }

    [Fact]
    public void UserId_returns_null_when_claim_invalid()
    {
        var identity = new ClaimsIdentity(new[]
        {
            new Claim("sub", "not-a-guid")
        }, "test");
        var context = new DefaultHttpContext { User = new ClaimsPrincipal(identity) };

        var accessor = new HttpContextAccessor { HttpContext = context };
        var service = new CurrentUserService(accessor);

        service.UserId.Should().BeNull();
    }
}
