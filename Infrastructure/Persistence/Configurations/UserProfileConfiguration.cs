using Domain.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Infrastructure.Persistence.Identity;

namespace Infrastructure.Persistence.Configurations;

public class UserProfileConfiguration : IEntityTypeConfiguration<UserProfile>
{
    public void Configure(EntityTypeBuilder<UserProfile> b)
    {
        b.ToTable("UserProfiles");

        // ✅ 主键：Id（来自 AuditableEntity）
        b.HasKey(x => x.Id);
        b.Property(x => x.Id).ValueGeneratedNever();

        // ✅ 外键：UserId（对应 AspNetUsers.Id）
        b.Property(x => x.UserId).IsRequired();
        b.HasIndex(x => x.UserId).IsUnique();

        b.Property(x => x.DisplayName).HasMaxLength(50);
        b.Property(x => x.Bio).HasMaxLength(500);
        b.Property(x => x.AvatarUrl).HasMaxLength(2048);
        b.Property(x => x.SteamProfileUrl).HasMaxLength(2048);
        b.Property(x => x.FaceitProfileUrl).HasMaxLength(2048);

        // FK -> AspNetUsers
        b.HasOne<ApplicationUser>()
            .WithOne()
            .HasForeignKey<UserProfile>(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
