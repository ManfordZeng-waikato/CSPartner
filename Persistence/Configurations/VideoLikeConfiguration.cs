using Domain.Videos;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Persistence.Identity;

namespace Persistence.Configurations;

public class VideoLikeConfiguration : IEntityTypeConfiguration<VideoLike>
{
    public void Configure(EntityTypeBuilder<VideoLike> b)
    {
        b.ToTable("VideoLikes");

        // ✅ 复合主键：防止重复点赞
        b.HasKey(x => new { x.VideoId, x.UserId });

        b.Property(x => x.VideoId).IsRequired();
        b.Property(x => x.UserId).IsRequired();

        b.HasIndex(x => x.UserId);

        // Video(1) - Likes(N)
        b.HasOne(x => x.Video)
            .WithMany(v => v.Likes)
            .HasForeignKey(x => x.VideoId)
            .OnDelete(DeleteBehavior.Cascade);

        // ✅ Like -> IdentityUser (FK)
        b.HasOne<ApplicationUser>()
            .WithMany()
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
