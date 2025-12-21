using Domain.Comments;
using Domain.Users;
using Domain.Videos;
using Microsoft.EntityFrameworkCore;

namespace Application.Common.Interfaces;

public interface IApplicationDbContext
{
    DbSet<UserProfile> UserProfiles { get; }
    DbSet<HighlightVideo> Videos { get; }
    DbSet<VideoLike> VideoLikes { get; }
    DbSet<Comment> Comments { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}

