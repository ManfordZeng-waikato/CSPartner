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

        // ===== AI Generated Fields =====
        b.Property(v => v.AiDescription)
            .HasMaxLength(600);

        b.Property(v => v.AiTagsJson)
            .HasColumnType("nvarchar(max)");

        b.Property(v => v.AiHighlightType)
            .HasConversion<int>()
            .IsRequired();

        b.Property(v => v.AiStatus)
            .HasConversion<int>()
            .IsRequired();

        b.Property(v => v.AiLastError)
            .HasMaxLength(1000);

        b.Property(v => v.AiUpdatedAtUtc)
            .HasColumnType("datetime2");

        // Index on AI status for efficient querying of pending videos
        b.HasIndex(v => v.AiStatus);

        // ✅ Reference to Identity user (no Domain.User needed)
        b.HasOne<ApplicationUser>()
            .WithMany()
            .HasForeignKey(x => x.UploaderUserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
