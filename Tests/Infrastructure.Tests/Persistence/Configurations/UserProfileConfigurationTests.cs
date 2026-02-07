using Domain.Users;
using FluentAssertions;
using Infrastructure.Persistence.Identity;
using Infrastructure.Tests.Helpers;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Tests.Persistence.Configurations;

public class UserProfileConfigurationTests
{
    [Fact]
    public void UserProfile_has_expected_configuration()
    {
        using var scope = TestDbContextScope.Create();
        var entity = scope.Context.Model.FindEntityType(typeof(UserProfile));

        entity.Should().NotBeNull();
        entity!.GetTableName().Should().Be("UserProfiles");

        var displayName = entity.FindProperty(nameof(UserProfile.DisplayName));
        displayName!.GetMaxLength().Should().Be(50);

        var bio = entity.FindProperty(nameof(UserProfile.Bio));
        bio!.GetMaxLength().Should().Be(500);

        var avatarUrl = entity.FindProperty(nameof(UserProfile.AvatarUrl));
        avatarUrl!.GetMaxLength().Should().Be(2048);

        var steamUrl = entity.FindProperty(nameof(UserProfile.SteamProfileUrl));
        steamUrl!.GetMaxLength().Should().Be(2048);

        var faceitUrl = entity.FindProperty(nameof(UserProfile.FaceitProfileUrl));
        faceitUrl!.GetMaxLength().Should().Be(2048);

        entity.GetIndexes().Should()
            .Contain(i => i.Properties.Any(p => p.Name == nameof(UserProfile.UserId)) && i.IsUnique);

        var userFk = entity.GetForeignKeys()
            .Single(fk => fk.Properties.Any(p => p.Name == nameof(UserProfile.UserId)));
        userFk.PrincipalEntityType.ClrType.Should().Be(typeof(ApplicationUser));
        userFk.DeleteBehavior.Should().Be(DeleteBehavior.Cascade);
    }
}
