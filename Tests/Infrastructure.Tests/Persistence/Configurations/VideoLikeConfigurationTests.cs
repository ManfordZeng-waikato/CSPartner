using Domain.Videos;
using FluentAssertions;
using Infrastructure.Persistence.Identity;
using Infrastructure.Tests.Helpers;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Tests.Persistence.Configurations;

public class VideoLikeConfigurationTests
{
    [Fact]
    public void VideoLike_has_expected_configuration()
    {
        using var scope = TestDbContextScope.Create();
        var entity = scope.Context.Model.FindEntityType(typeof(VideoLike));

        entity.Should().NotBeNull();
        entity!.GetTableName().Should().Be("VideoLikes");

        var key = entity.FindPrimaryKey();
        key!.Properties.Select(p => p.Name).Should().BeEquivalentTo(new[]
        {
            nameof(VideoLike.VideoId),
            nameof(VideoLike.UserId)
        });

        entity.GetIndexes().Should()
            .Contain(i => i.Properties.Any(p => p.Name == nameof(VideoLike.UserId)));

        var videoFk = entity.GetForeignKeys()
            .Single(fk => fk.Properties.Any(p => p.Name == nameof(VideoLike.VideoId)));
        videoFk.PrincipalEntityType.ClrType.Should().Be(typeof(HighlightVideo));
        videoFk.DeleteBehavior.Should().Be(DeleteBehavior.Cascade);

        var userFk = entity.GetForeignKeys()
            .Single(fk => fk.Properties.Any(p => p.Name == nameof(VideoLike.UserId)));
        userFk.PrincipalEntityType.ClrType.Should().Be(typeof(ApplicationUser));
        userFk.DeleteBehavior.Should().Be(DeleteBehavior.Restrict);
    }
}
