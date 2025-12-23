using Domain.Videos;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Infrastructure.Persistence.Identity;

namespace Infrastructure.Persistence.Configurations;

public class HighlightVideoConfiguration : IEntityTypeConfiguration<HighlightVideo>
{
    public void Configure(EntityTypeBuilder<HighlightVideo> b)
    {
        b.ToTable("Videos");

        b.HasKey(x => x.Id);
        b.Property(x => x.Id).ValueGeneratedNever();

        b.Property(x => x.UploaderUserId).IsRequired();

        b.Property(x => x.Title)
            .HasMaxLength(120)
            .IsRequired();

        b.Property(x => x.Description)
            .HasMaxLength(2000);

        b.Property(x => x.VideoUrl)
            .HasMaxLength(2048)
            .IsRequired();

        b.Property(x => x.ThumbnailUrl)
            .HasMaxLength(2048);

        b.Property(x => x.Visibility)
            .HasConversion<int>()
            .IsRequired();

        b.HasIndex(x => x.CreatedAtUtc);
        b.HasIndex(x => x.UploaderUserId);

        // ✅ Reference to Identity user (no Domain.User needed)
        b.HasOne<ApplicationUser>()
            .WithMany()
            .HasForeignKey(x => x.UploaderUserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
