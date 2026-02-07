using FluentAssertions;
using Infrastructure.Identity;
using Microsoft.Extensions.Logging.Abstractions;

namespace Infrastructure.Tests.Identity;

public class TokenBlacklistServiceTests
{
    [Fact]
    public async Task Add_and_check_blacklist_returns_true()
    {
        var service = new TokenBlacklistService(NullLogger<TokenBlacklistService>.Instance);
        var token = "token";

        await service.AddToBlacklistAsync(token, DateTime.UtcNow.AddMinutes(5));

        (await service.IsTokenBlacklistedAsync(token)).Should().BeTrue();
    }

    [Fact]
    public async Task Expired_token_is_not_blacklisted()
    {
        var service = new TokenBlacklistService(NullLogger<TokenBlacklistService>.Instance);
        var token = "token";

        await service.AddToBlacklistAsync(token, DateTime.UtcNow.AddMinutes(-1));

        (await service.IsTokenBlacklistedAsync(token)).Should().BeFalse();
    }

    [Fact]
    public async Task Remove_blacklist_clears_token()
    {
        var service = new TokenBlacklistService(NullLogger<TokenBlacklistService>.Instance);
        var token = "token";
        await service.AddToBlacklistAsync(token, DateTime.UtcNow.AddMinutes(5));

        await service.RemoveFromBlacklistAsync(token);

        (await service.IsTokenBlacklistedAsync(token)).Should().BeFalse();
    }
}
