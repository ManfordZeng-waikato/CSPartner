using Application.Features.Videos.Queries.GetVideosByUserId;
using Application.Tests.Helpers;
using Domain.Videos;
using FluentAssertions;

namespace Application.Tests;

public class GetVideosByUserIdQueryHandlerTests
{
    [Fact]
    public async Task Handle_filters_private_when_viewing_other_user()
    {
        using var scope = TestDbContextScope.Create();
        var context = scope.Context;

        var userId = Guid.NewGuid();
        var publicVideo = new HighlightVideo(userId, "pub", "url1");
        var privateVideo = new HighlightVideo(userId, "priv", "url2");
        privateVideo.SetVisibility(VideoVisibility.Private);
        context.Videos.AddRange(publicVideo, privateVideo);
        await context.SaveChangesAsync();

        var handler = new GetVideosByUserIdQueryHandler(context);

        var result = await handler.Handle(
            new GetVideosByUserIdQuery(userId, CurrentUserId: Guid.NewGuid()),
            CancellationToken.None);

        result.Select(v => v.VideoId).Should().Contain(publicVideo.VideoId);
        result.Select(v => v.VideoId).Should().NotContain(privateVideo.VideoId);
    }

    [Fact]
    public async Task Handle_includes_private_when_viewing_own_videos()
    {
        using var scope = TestDbContextScope.Create();
        var context = scope.Context;

        var userId = Guid.NewGuid();
        var publicVideo = new HighlightVideo(userId, "pub", "url1");
        var privateVideo = new HighlightVideo(userId, "priv", "url2");
        privateVideo.SetVisibility(VideoVisibility.Private);
        context.Videos.AddRange(publicVideo, privateVideo);
        await context.SaveChangesAsync();

        var handler = new GetVideosByUserIdQueryHandler(context);

        var result = await handler.Handle(
            new GetVideosByUserIdQuery(userId, CurrentUserId: userId),
            CancellationToken.None);

        result.Select(v => v.VideoId).Should().Contain(publicVideo.VideoId);
        result.Select(v => v.VideoId).Should().Contain(privateVideo.VideoId);
    }
}
