using Domain.Comments;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Infrastructure.Persistence.Identity;

namespace Infrastructure.Persistence.Configurations;

public class CommentConfiguration : IEntityTypeConfiguration<Comment>
{
    public void Configure(EntityTypeBuilder<Comment> b)
    {
        b.ToTable("Comments");

        b.HasKey(x => x.Id);
        b.Property(x => x.Id).ValueGeneratedNever();

        b.Property(x => x.VideoId).IsRequired();
        b.Property(x => x.UserId).IsRequired();

        b.Property(x => x.Content)
            .HasMaxLength(2000)
            .IsRequired();

        b.HasIndex(x => new { x.VideoId, x.CreatedAtUtc });

        // Video(1) - Comment(N)
        b.HasOne(x => x.Video)
            .WithMany(v => v.Comments)
            .HasForeignKey(x => x.VideoId)
            .OnDelete(DeleteBehavior.Cascade);

        // ✅ Comment -> IdentityUser (FK)
        b.HasOne<ApplicationUser>()
            .WithMany()
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        // ✅ Nested comment self-reference: prevent cascade delete
        b.HasOne(x => x.ParentComment)
            .WithMany(p => p.Replies)
            .HasForeignKey(x => x.ParentCommentId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
