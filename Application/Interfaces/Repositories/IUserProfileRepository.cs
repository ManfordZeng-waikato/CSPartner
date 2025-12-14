using Domain.Users;

namespace Application.Interfaces.Repositories;

public interface IUserProfileRepository
{
    Task<UserProfile?> GetByIdAsync(Guid id);
    Task<UserProfile?> GetByUserIdAsync(Guid userId);
    Task<UserProfile> AddAsync(UserProfile profile);
    Task UpdateAsync(UserProfile profile);
}
