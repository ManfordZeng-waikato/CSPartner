using Application.Features.Videos.Commands.IncreaseViewCount;
using Application.Tests.Helpers;
using Domain.Videos;
using FluentAssertions;

namespace Application.Tests;

public class IncreaseViewCountCommandHandlerTests
{
    [Fact]
    public async Task Handle_increments_view_count_when_video_exists()
    {
        using var scope = TestDbContextScope.Create();
        var context = scope.Context;

        var video = new HighlightVideo(Guid.NewGuid(), "t", "url");
        context.Videos.Add(video);
        await context.SaveChangesAsync();

        var handler = new IncreaseViewCountCommandHandler(context);

        await handler.Handle(new IncreaseViewCountCommand(video.VideoId), CancellationToken.None);

        await context.Entry(video).ReloadAsync();
        var updated = await context.Videos.FindAsync(video.VideoId);
        updated!.ViewCount.Should().Be(1);
    }

    [Fact]
    public async Task Handle_does_not_throw_when_video_missing()
    {
        using var scope = TestDbContextScope.Create();
        var context = scope.Context;

        var handler = new IncreaseViewCountCommandHandler(context);

        var act = async () => await handler.Handle(new IncreaseViewCountCommand(Guid.NewGuid()), CancellationToken.None);

        await act.Should().NotThrowAsync();
    }
}
