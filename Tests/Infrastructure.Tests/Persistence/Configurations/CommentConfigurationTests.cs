using Domain.Comments;
using Domain.Videos;
using FluentAssertions;
using Infrastructure.Persistence.Identity;
using Infrastructure.Tests.Helpers;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Tests.Persistence.Configurations;

public class CommentConfigurationTests
{
    [Fact]
    public void Comment_has_expected_configuration()
    {
        using var scope = TestDbContextScope.Create();
        var entity = scope.Context.Model.FindEntityType(typeof(Comment));

        entity.Should().NotBeNull();
        entity!.GetTableName().Should().Be("Comments");

        var content = entity.FindProperty(nameof(Comment.Content));
        content!.GetMaxLength().Should().Be(2000);
        content.IsNullable.Should().BeFalse();

        entity.GetIndexes().Should()
            .Contain(i => i.Properties.Select(p => p.Name).SequenceEqual(new[]
            {
                nameof(Comment.VideoId),
                nameof(Comment.CreatedAtUtc)
            }));

        var videoFk = entity.GetForeignKeys()
            .Single(fk => fk.Properties.Any(p => p.Name == nameof(Comment.VideoId)));
        videoFk.PrincipalEntityType.ClrType.Should().Be(typeof(HighlightVideo));
        videoFk.DeleteBehavior.Should().Be(DeleteBehavior.Cascade);

        var userFk = entity.GetForeignKeys()
            .Single(fk => fk.Properties.Any(p => p.Name == nameof(Comment.UserId)));
        userFk.PrincipalEntityType.ClrType.Should().Be(typeof(ApplicationUser));
        userFk.DeleteBehavior.Should().Be(DeleteBehavior.Restrict);

        var parentFk = entity.GetForeignKeys()
            .Single(fk => fk.Properties.Any(p => p.Name == nameof(Comment.ParentCommentId)));
        parentFk.PrincipalEntityType.ClrType.Should().Be(typeof(Comment));
        parentFk.DeleteBehavior.Should().Be(DeleteBehavior.Restrict);
    }
}
