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

        // ✅ Primary key: Id (from AuditableEntity)
        b.HasKey(x => x.Id);
        b.Property(x => x.Id).ValueGeneratedNever();

        // ✅ Foreign key: UserId (corresponds to AspNetUsers.Id)
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
