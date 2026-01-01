using Application.Common.Interfaces;
using System.Collections.Concurrent;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Identity;

/// <summary>
/// In-memory implementation of token blacklist service
/// For production with high load, consider using Redis instead
/// </summary>
public class TokenBlacklistService : ITokenBlacklistService
{
    // Thread-safe dictionary to store blacklisted token hashes
    // Key: token hash, Value: expiration time
    private readonly ConcurrentDictionary<string, DateTime> _blacklist = new();
    private readonly ILogger<TokenBlacklistService> _logger;
    private readonly Timer _cleanupTimer;

    public TokenBlacklistService(ILogger<TokenBlacklistService> logger)
    {
        _logger = logger;
        
        // Cleanup expired tokens every 5 minutes
        _cleanupTimer = new Timer(CleanupExpiredTokens, null, TimeSpan.FromMinutes(5), TimeSpan.FromMinutes(5));
    }

    /// <summary>
    /// Compute SHA256 hash of token for storage (don't store full token for security)
    /// </summary>
    private static string ComputeTokenHash(string token)
    {
        var bytes = Encoding.UTF8.GetBytes(token);
        var hash = SHA256.HashData(bytes);
        return Convert.ToBase64String(hash);
    }

    public Task<bool> IsTokenBlacklistedAsync(string token)
    {
        if (string.IsNullOrWhiteSpace(token))
            return Task.FromResult(false);

        var tokenHash = ComputeTokenHash(token);
        
        if (_blacklist.TryGetValue(tokenHash, out var expiresAt))
        {
            // Check if token is still blacklisted (not expired)
            if (expiresAt > DateTime.UtcNow)
            {
                return Task.FromResult(true);
            }
            
            // Token expired, remove from blacklist
            _blacklist.TryRemove(tokenHash, out _);
        }

        return Task.FromResult(false);
    }

    public Task AddToBlacklistAsync(string token, DateTime expiresAt)
    {
        if (string.IsNullOrWhiteSpace(token))
            return Task.CompletedTask;

        // Only add if token hasn't already expired
        if (expiresAt <= DateTime.UtcNow)
        {
            _logger.LogDebug("Token already expired, skipping blacklist");
            return Task.CompletedTask;
        }

        var tokenHash = ComputeTokenHash(token);
        _blacklist.AddOrUpdate(tokenHash, expiresAt, (key, oldValue) => expiresAt);

        _logger.LogDebug("Token added to blacklist, expires at {ExpiresAt}", expiresAt);
        return Task.CompletedTask;
    }

    public Task RemoveFromBlacklistAsync(string token)
    {
        if (string.IsNullOrWhiteSpace(token))
            return Task.CompletedTask;

        var tokenHash = ComputeTokenHash(token);
        _blacklist.TryRemove(tokenHash, out _);

        _logger.LogDebug("Token removed from blacklist");
        return Task.CompletedTask;
    }

    /// <summary>
    /// Cleanup expired tokens from blacklist to prevent memory leak
    /// </summary>
    private void CleanupExpiredTokens(object? state)
    {
        var now = DateTime.UtcNow;
        var expiredKeys = _blacklist
            .Where(kvp => kvp.Value <= now)
            .Select(kvp => kvp.Key)
            .ToList();

        foreach (var key in expiredKeys)
        {
            _blacklist.TryRemove(key, out _);
        }

        if (expiredKeys.Count > 0)
        {
            _logger.LogDebug("Cleaned up {Count} expired tokens from blacklist", expiredKeys.Count);
        }
    }

    /// <summary>
    /// Get current blacklist size (for monitoring)
    /// </summary>
    public int GetBlacklistSize() => _blacklist.Count;
}

