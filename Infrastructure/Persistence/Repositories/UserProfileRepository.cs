using Application.Interfaces.Repositories;
using Domain.Users;
using Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories;

public class UserProfileRepository : IUserProfileRepository
{
    private readonly AppDbContext _context;

    public UserProfileRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<UserProfile?> GetByIdAsync(Guid id)
    {
        return await _context.UserProfiles.FindAsync(id);
    }

    public async Task<UserProfile?> GetByUserIdAsync(Guid userId)
    {
        return await _context.UserProfiles
            .FirstOrDefaultAsync(p => p.UserId == userId);
    }

    public async Task<UserProfile> AddAsync(UserProfile profile)
    {
        await _context.UserProfiles.AddAsync(profile);
        return profile;
    }

    public Task UpdateAsync(UserProfile profile)
    {
        _context.UserProfiles.Update(profile);
        return Task.CompletedTask;
    }
}
