namespace Application.Common.Interfaces;

/// <summary>
/// Service for managing blacklisted JWT tokens
/// Tokens added to the blacklist will be rejected even if they are still valid
/// </summary>
public interface ITokenBlacklistService
{
    /// <summary>
    /// Check if a token is blacklisted
    /// </summary>
    /// <param name="token">The JWT token to check</param>
    /// <returns>True if the token is blacklisted, false otherwise</returns>
    Task<bool> IsTokenBlacklistedAsync(string token);

    /// <summary>
    /// Add a token to the blacklist until it expires
    /// </summary>
    /// <param name="token">The JWT token to blacklist</param>
    /// <param name="expiresAt">The expiration time of the token</param>
    Task AddToBlacklistAsync(string token, DateTime expiresAt);

    /// <summary>
    /// Remove a token from the blacklist (useful for testing or edge cases)
    /// </summary>
    /// <param name="token">The JWT token to remove from blacklist</param>
    Task RemoveFromBlacklistAsync(string token);
}

