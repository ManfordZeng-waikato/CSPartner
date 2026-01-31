using Domain.Ai;
using Domain.Videos;
using FluentAssertions;

namespace Domain.Tests;

public class HighlightVideoTests
{
    [Fact]
    public void Ctor_sets_core_fields_and_defaults()
    {
        var uploaderId = Guid.NewGuid();
        var video = new HighlightVideo(
            uploaderId,
            "  My Title  ",
            "  https://video  ",
            "  desc  ",
            "  https://thumb  ");

        video.UploaderUserId.Should().Be(uploaderId);
        video.Title.Should().Be("My Title");
        video.VideoUrl.Should().Be("https://video");
        video.Description.Should().Be("desc");
        video.ThumbnailUrl.Should().Be("https://thumb");
        video.LikeCount.Should().Be(0);
        video.CommentCount.Should().Be(0);
        video.ViewCount.Should().Be(0);
        video.Visibility.Should().Be(VideoVisibility.Public);
        video.IsDeleted.Should().BeFalse();
    }

    [Fact]
    public void SetTitle_trims_and_limits_length()
    {
        var video = new HighlightVideo(Guid.NewGuid(), "t", "url");
        var longTitle = new string('a', 130);

        video.SetTitle($"  {longTitle}  ");

        video.Title.Should().Be(longTitle[..120]);
    }

    [Fact]
    public void SetTitle_throws_when_empty()
    {
        var video = new HighlightVideo(Guid.NewGuid(), "t", "url");

        var act = () => video.SetTitle("  ");

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void MarkAiCompleted_trims_and_sets_status()
    {
        var video = new HighlightVideo(Guid.NewGuid(), "t", "url");
        var longDesc = new string('d', 700);

        video.MarkAiCompleted($"  {longDesc}  ");

        video.AiStatus.Should().Be(AiStatus.Completed);
        video.AiLastError.Should().BeNull();
        video.AiDescription.Should().Be(longDesc[..600]);
        video.AiUpdatedAtUtc.Should().NotBeNull();
    }

    [Fact]
    public void MarkAiFailed_trims_and_sets_status()
    {
        var video = new HighlightVideo(Guid.NewGuid(), "t", "url");
        var longError = new string('e', 1200);

        video.MarkAiFailed($"  {longError}  ");

        video.AiStatus.Should().Be(AiStatus.Failed);
        video.AiLastError.Should().Be(longError[..1000]);
        video.AiUpdatedAtUtc.Should().NotBeNull();
    }

    [Fact]
    public void ApplyLikeRemoved_does_not_go_below_zero()
    {
        var video = new HighlightVideo(Guid.NewGuid(), "t", "url");

        video.ApplyLikeRemoved();

        video.LikeCount.Should().Be(0);
    }

    [Fact]
    public void SetTags_throws_when_empty()
    {
        var video = new HighlightVideo(Guid.NewGuid(), "t", "url");

        var act = () => video.SetTags(" ");

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void SetDescription_null_clears_description()
    {
        var video = new HighlightVideo(Guid.NewGuid(), "t", "url", "desc");

        video.SetDescription(null);

        video.Description.Should().BeNull();
    }
}
