using Application.Common.Interfaces;
using Application.Features.Videos.Commands.UpdateVideo;
using Application.Tests.Helpers;
using Domain.Exceptions;
using Domain.Videos;
using FluentAssertions;
using Moq;

namespace Application.Tests;

public class UpdateVideoCommandHandlerTests
{
    [Fact]
    public async Task Handle_throws_when_not_authenticated()
    {
        using var scope = TestDbContextScope.Create();
        var context = scope.Context;

        var currentUser = new Mock<ICurrentUserService>();
        currentUser.SetupGet(c => c.UserId).Returns((Guid?)null);

        var handler = new UpdateVideoCommandHandler(context, currentUser.Object);

        var act = async () => await handler.Handle(
            new UpdateVideoCommand(Guid.NewGuid(), VideoVisibility.Private),
            CancellationToken.None);

        await act.Should().ThrowAsync<AuthenticationRequiredException>();
    }

    [Fact]
    public async Task Handle_throws_when_video_missing()
    {
        using var scope = TestDbContextScope.Create();
        var context = scope.Context;

        var currentUser = new Mock<ICurrentUserService>();
        currentUser.SetupGet(c => c.UserId).Returns(Guid.NewGuid());

        var handler = new UpdateVideoCommandHandler(context, currentUser.Object);

        var act = async () => await handler.Handle(
            new UpdateVideoCommand(Guid.NewGuid(), VideoVisibility.Private),
            CancellationToken.None);

        await act.Should().ThrowAsync<VideoNotFoundException>();
    }

    [Fact]
    public async Task Handle_throws_when_not_owner()
    {
        using var scope = TestDbContextScope.Create();
        var context = scope.Context;

        var otherUserId = Guid.NewGuid();
        var video = new HighlightVideo(otherUserId, "t", "url");
        context.Videos.Add(video);
        await context.SaveChangesAsync();

        var currentUser = new Mock<ICurrentUserService>();
        currentUser.SetupGet(c => c.UserId).Returns(Guid.NewGuid());

        var handler = new UpdateVideoCommandHandler(context, currentUser.Object);

        var act = async () => await handler.Handle(
            new UpdateVideoCommand(video.VideoId, VideoVisibility.Private),
            CancellationToken.None);

        await act.Should().ThrowAsync<UnauthorizedOperationException>();
    }

    [Fact]
    public async Task Handle_updates_visibility_when_owner()
    {
        using var scope = TestDbContextScope.Create();
        var context = scope.Context;

        var userId = Guid.NewGuid();
        var video = new HighlightVideo(userId, "t", "url");
        context.Videos.Add(video);
        await context.SaveChangesAsync();

        var currentUser = new Mock<ICurrentUserService>();
        currentUser.SetupGet(c => c.UserId).Returns(userId);

        var handler = new UpdateVideoCommandHandler(context, currentUser.Object);

        await handler.Handle(
            new UpdateVideoCommand(video.VideoId, VideoVisibility.Private),
            CancellationToken.None);

        var updated = await context.Videos.FindAsync(video.VideoId);
        updated!.Visibility.Should().Be(VideoVisibility.Private);
    }
}
