using Domain.Videos;
using FluentAssertions;
using Infrastructure.Persistence.Identity;
using Infrastructure.Tests.Helpers;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Tests.Persistence.Configurations;

public class HighlightVideoConfigurationTests
{
    [Fact]
    public void HighlightVideo_has_expected_configuration()
    {
        using var scope = TestDbContextScope.Create();
        var entity = scope.Context.Model.FindEntityType(typeof(HighlightVideo));

        entity.Should().NotBeNull();
        entity!.GetTableName().Should().Be("Videos");

        var title = entity.FindProperty(nameof(HighlightVideo.Title));
        title!.GetMaxLength().Should().Be(120);
        title.IsNullable.Should().BeFalse();

        var description = entity.FindProperty(nameof(HighlightVideo.Description));
        description!.GetMaxLength().Should().Be(2000);
        description.IsNullable.Should().BeTrue();

        var videoUrl = entity.FindProperty(nameof(HighlightVideo.VideoUrl));
        videoUrl!.GetMaxLength().Should().Be(2048);
        videoUrl.IsNullable.Should().BeFalse();

        var thumbnailUrl = entity.FindProperty(nameof(HighlightVideo.ThumbnailUrl));
        thumbnailUrl!.GetMaxLength().Should().Be(2048);

        var aiDescription = entity.FindProperty(nameof(HighlightVideo.AiDescription));
        aiDescription!.GetMaxLength().Should().Be(600);

        var aiLastError = entity.FindProperty(nameof(HighlightVideo.AiLastError));
        aiLastError!.GetMaxLength().Should().Be(1000);

        var tagsJson = entity.FindProperty(nameof(HighlightVideo.TagsJson));
        tagsJson!.GetColumnType().Should().Be("nvarchar(max)");

        var visibility = entity.FindProperty(nameof(HighlightVideo.Visibility));
        visibility!.GetProviderClrType().Should().Be(typeof(int));

        var aiStatus = entity.FindProperty(nameof(HighlightVideo.AiStatus));
        aiStatus!.GetProviderClrType().Should().Be(typeof(int));

        var aiHighlightType = entity.FindProperty(nameof(HighlightVideo.AiHighlightType));
        aiHighlightType!.GetProviderClrType().Should().Be(typeof(int));

        entity.GetIndexes().Should().Contain(i => i.Properties.Any(p => p.Name == nameof(HighlightVideo.CreatedAtUtc)));
        entity.GetIndexes().Should().Contain(i => i.Properties.Any(p => p.Name == nameof(HighlightVideo.UploaderUserId)));
        entity.GetIndexes().Should().Contain(i => i.Properties.Any(p => p.Name == nameof(HighlightVideo.AiStatus)));

        var uploaderFk = entity.GetForeignKeys()
            .Single(fk => fk.Properties.Any(p => p.Name == nameof(HighlightVideo.UploaderUserId)));
        uploaderFk.PrincipalEntityType.ClrType.Should().Be(typeof(ApplicationUser));
        uploaderFk.DeleteBehavior.Should().Be(DeleteBehavior.Restrict);
    }
}
