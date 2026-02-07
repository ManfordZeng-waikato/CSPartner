using Application.Features.Videos.Queries.GetVideos;
using Application.Tests.Helpers;
using Domain.Videos;
using FluentAssertions;

namespace Application.Tests;

public class GetVideosQueryHandlerTests
{
    [Fact]
    public async Task Handle_returns_public_and_own_private_videos()
    {
        using var scope = TestDbContextScope.Create();
        var context = scope.Context;

        var currentUserId = Guid.NewGuid();
        var otherUserId = Guid.NewGuid();

        var publicOther = new HighlightVideo(otherUserId, "pub", "url1");
        var privateOther = new HighlightVideo(otherUserId, "priv", "url2");
        privateOther.SetVisibility(VideoVisibility.Private);

        var privateOwn = new HighlightVideo(currentUserId, "mine", "url3");
        privateOwn.SetVisibility(VideoVisibility.Private);

        context.Videos.AddRange(publicOther, privateOther, privateOwn);
        await context.SaveChangesAsync();

        var handler = new GetVideosQueryHandler(context);

        var result = await handler.Handle(
            new GetVideosQuery(Cursor: null, PageSize: 20, CurrentUserId: currentUserId),
            CancellationToken.None);

        var ids = result.Items.Select(v => v.VideoId).ToList();
        ids.Should().Contain(publicOther.VideoId);
        ids.Should().Contain(privateOwn.VideoId);
        ids.Should().NotContain(privateOther.VideoId);
    }

    [Fact]
    public async Task Handle_sets_has_liked_when_user_liked()
    {
        using var scope = TestDbContextScope.Create();
        var context = scope.Context;

        var currentUserId = Guid.NewGuid();
        var video = new HighlightVideo(currentUserId, "t", "url");
        context.Videos.Add(video);
        context.VideoLikes.Add(new VideoLike(video.VideoId, currentUserId));
        await context.SaveChangesAsync();

        var handler = new GetVideosQueryHandler(context);

        var result = await handler.Handle(
            new GetVideosQuery(Cursor: null, PageSize: 20, CurrentUserId: currentUserId),
            CancellationToken.None);

        result.Items.Should().ContainSingle(v => v.VideoId == video.VideoId && v.HasLiked);
    }
}
