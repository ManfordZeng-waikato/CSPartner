using Domain.Users;
using Microsoft.EntityFrameworkCore;
using Persistence.Context;

namespace Application.Services;

public class UserProfileService : IUserProfileService
{
    private readonly AppDbContext _context;

    public UserProfileService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<UserProfile?> GetUserProfileAsync(Guid userId)
    {
        return await _context.UserProfiles.FindAsync(userId);
    }

    public async Task<UserProfile?> GetUserProfileByUserIdAsync(Guid userId)
    {
        return await _context.UserProfiles
            .FirstOrDefaultAsync(p => p.UserId == userId);
    }

    public async Task<UserProfile> CreateOrUpdateUserProfileAsync(Guid userId, string? displayName, string? bio, string? avatarUrl, string? steamUrl, string? faceitUrl)
    {
        var profile = await _context.UserProfiles
            .FirstOrDefaultAsync(p => p.UserId == userId);

        if (profile == null)
        {
            profile = new UserProfile(userId);
            _context.UserProfiles.Add(profile);
        }

        profile.Update(displayName, bio, avatarUrl, steamUrl, faceitUrl);
        await _context.SaveChangesAsync();
        return profile;
    }
}

