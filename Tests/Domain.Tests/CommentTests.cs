using Domain.Comments;
using FluentAssertions;

namespace Domain.Tests;

public class CommentTests
{
    [Fact]
    public void Ctor_trims_and_limits_content()
    {
        var longContent = new string('c', 2500);
        var comment = new Comment(Guid.NewGuid(), Guid.NewGuid(), $"  {longContent}  ");

        comment.Content.Should().Be(longContent[..2000]);
    }

    [Fact]
    public void SetContent_throws_when_empty()
    {
        var comment = new Comment(Guid.NewGuid(), Guid.NewGuid(), "ok");

        var act = () => comment.SetContent(" ");

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void SetContent_trims_and_limits()
    {
        var comment = new Comment(Guid.NewGuid(), Guid.NewGuid(), "ok");
        var longContent = new string('c', 2100);

        comment.SetContent($"  {longContent}  ");

        comment.Content.Should().Be(longContent[..2000]);
    }

    [Fact]
    public void SoftDelete_sets_deleted_state_and_content()
    {
        var comment = new Comment(Guid.NewGuid(), Guid.NewGuid(), "hello");

        comment.SoftDelete("  spam  ");

        comment.IsDeleted.Should().BeTrue();
        comment.DeletedReason.Should().Be("spam");
        comment.Content.Should().Be("This comment has been deleted");
    }

    [Fact]
    public void SoftDelete_null_reason_clears_reason()
    {
        var comment = new Comment(Guid.NewGuid(), Guid.NewGuid(), "hello");

        comment.SoftDelete(null);

        comment.DeletedReason.Should().BeNull();
    }
}
