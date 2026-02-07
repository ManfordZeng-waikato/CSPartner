using Application.Features.Videos.Queries.GetVideoById;
using Application.Tests.Helpers;
using Domain.Videos;
using FluentAssertions;

namespace Application.Tests;

public class GetVideoByIdQueryHandlerTests
{
    [Fact]
    public async Task Handle_returns_null_for_private_when_not_owner()
    {
        using var scope = TestDbContextScope.Create();
        var context = scope.Context;

        var otherUserId = Guid.NewGuid();
        var video = new HighlightVideo(otherUserId, "t", "url");
        video.SetVisibility(VideoVisibility.Private);
        context.Videos.Add(video);
        await context.SaveChangesAsync();

        var handler = new GetVideoByIdQueryHandler(context);

        var result = await handler.Handle(
            new GetVideoByIdQuery(video.VideoId, Guid.NewGuid()),
            CancellationToken.None);

        result.Should().BeNull();
    }

    [Fact]
    public async Task Handle_returns_video_for_owner()
    {
        using var scope = TestDbContextScope.Create();
        var context = scope.Context;

        var ownerId = Guid.NewGuid();
        var video = new HighlightVideo(ownerId, "t", "url");
        video.SetVisibility(VideoVisibility.Private);
        context.Videos.Add(video);
        await context.SaveChangesAsync();

        var handler = new GetVideoByIdQueryHandler(context);

        var result = await handler.Handle(
            new GetVideoByIdQuery(video.VideoId, ownerId),
            CancellationToken.None);

        result.Should().NotBeNull();
        result!.VideoId.Should().Be(video.VideoId);
    }
}
