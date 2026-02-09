using Application.Common.Interfaces;
using System.Collections.Generic;

namespace Infrastructure.Tests.Helpers;

public class FakeEmailService : IEmailService
{
    public Task SendConfirmationLinkAsync(string email, string userName, string link) => Task.CompletedTask;
    public Task SendPasswordResetLinkAsync(string email, string userName, string link) => Task.CompletedTask;
    public Task SendPasswordResetCodeAsync(string email, string userName, string code) => Task.CompletedTask;
    public Task SendEmailAsync(string email, string subject, string htmlMessage) => Task.CompletedTask;
}

public class FakeTokenBlacklistService : ITokenBlacklistService
{
    public List<(string Token, DateTime ExpiresAt)> AddedTokens { get; } = new();

    public Task<bool> IsTokenBlacklistedAsync(string token) => Task.FromResult(false);

    public Task AddToBlacklistAsync(string token, DateTime expiresAt)
    {
        AddedTokens.Add((token, expiresAt));
        return Task.CompletedTask;
    }

    public Task RemoveFromBlacklistAsync(string token) => Task.CompletedTask;
}

public class FakeCurrentUserService : ICurrentUserService
{
    public Guid? UserId { get; set; }
    public bool IsAuthenticated => UserId.HasValue;
}
