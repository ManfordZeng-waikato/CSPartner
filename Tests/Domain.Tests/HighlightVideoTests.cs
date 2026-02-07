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
    public void MarkAiFailed_throws_when_empty()
    {
        var video = new HighlightVideo(Guid.NewGuid(), "t", "url");

        var act = () => video.MarkAiFailed(" ");

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void MarkAiPending_clears_error()
    {
        var video = new HighlightVideo(Guid.NewGuid(), "t", "url");
        video.MarkAiFailed("err");

        video.MarkAiPending();

        video.AiStatus.Should().Be(AiStatus.Pending);
        video.AiLastError.Should().BeNull();
    }

    [Fact]
    public void ApplyLikeRemoved_does_not_go_below_zero()
    {
        var video = new HighlightVideo(Guid.NewGuid(), "t", "url");

        video.ApplyLikeRemoved();

        video.LikeCount.Should().Be(0);
    }

    [Fact]
    public void ApplyLikeAdded_increments_count()
    {
        var video = new HighlightVideo(Guid.NewGuid(), "t", "url");

        video.ApplyLikeAdded();

        video.LikeCount.Should().Be(1);
    }

    [Fact]
    public void ApplyCommentAdded_and_removed_update_count()
    {
        var video = new HighlightVideo(Guid.NewGuid(), "t", "url");

        video.ApplyCommentAdded();
        video.ApplyCommentRemoved();

        video.CommentCount.Should().Be(0);
    }

    [Fact]
    public void SetTags_throws_when_empty()
    {
        var video = new HighlightVideo(Guid.NewGuid(), "t", "url");

        var act = () => video.SetTags(" ");

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void SetTags_sets_value()
    {
        var video = new HighlightVideo(Guid.NewGuid(), "t", "url");

        video.SetTags("[\"Mirage\",\"AK47\"]");

        video.TagsJson.Should().Be("[\"Mirage\",\"AK47\"]");
    }

    [Fact]
    public void SetDescription_null_clears_description()
    {
        var video = new HighlightVideo(Guid.NewGuid(), "t", "url", "desc");

        video.SetDescription(null);

        video.Description.Should().BeNull();
    }

    [Fact]
    public void SetVideoUrl_throws_when_empty()
    {
        var video = new HighlightVideo(Guid.NewGuid(), "t", "url");

        var act = () => video.SetVideoUrl(" ");

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void SetThumbnailUrl_normalizes_empty_to_null()
    {
        var video = new HighlightVideo(Guid.NewGuid(), "t", "url", thumbnailUrl: "x");

        video.SetThumbnailUrl(" ");

        video.ThumbnailUrl.Should().BeNull();
    }

    [Fact]
    public void IncreaseView_increments_count()
    {
        var video = new HighlightVideo(Guid.NewGuid(), "t", "url");

        video.IncreaseView();

        video.ViewCount.Should().Be(1);
    }

    [Fact]
    public void SetVisibility_updates_value()
    {
        var video = new HighlightVideo(Guid.NewGuid(), "t", "url");

        video.SetVisibility(VideoVisibility.Private);

        video.Visibility.Should().Be(VideoVisibility.Private);
    }

    [Fact]
    public void SoftDelete_sets_flag()
    {
        var video = new HighlightVideo(Guid.NewGuid(), "t", "url");

        video.SoftDelete();

        video.IsDeleted.Should().BeTrue();
    }
}
