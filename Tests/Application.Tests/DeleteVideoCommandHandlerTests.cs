using Application.Common.Interfaces;
using Application.Features.Videos.Commands.DeleteVideo;
using Application.Tests.Helpers;
using Domain.Exceptions;
using Domain.Videos;
using FluentAssertions;
using Moq;

namespace Application.Tests;

public class DeleteVideoCommandHandlerTests
{
    [Fact]
    public async Task Handle_throws_when_not_authenticated()
    {
        using var scope = TestDbContextScope.Create();
        var context = scope.Context;

        var currentUser = new Mock<ICurrentUserService>();
        currentUser.SetupGet(c => c.UserId).Returns((Guid?)null);

        var handler = new DeleteVideoCommandHandler(context, currentUser.Object);

        var act = async () => await handler.Handle(
            new DeleteVideoCommand(Guid.NewGuid()),
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

        var handler = new DeleteVideoCommandHandler(context, currentUser.Object);

        var act = async () => await handler.Handle(
            new DeleteVideoCommand(Guid.NewGuid()),
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

        var handler = new DeleteVideoCommandHandler(context, currentUser.Object);

        var act = async () => await handler.Handle(
            new DeleteVideoCommand(video.VideoId),
            CancellationToken.None);

        await act.Should().ThrowAsync<UnauthorizedOperationException>();
    }

    [Fact]
    public async Task Handle_soft_deletes_when_owner()
    {
        using var scope = TestDbContextScope.Create();
        var context = scope.Context;

        var userId = Guid.NewGuid();
        var video = new HighlightVideo(userId, "t", "url");
        context.Videos.Add(video);
        await context.SaveChangesAsync();

        var currentUser = new Mock<ICurrentUserService>();
        currentUser.SetupGet(c => c.UserId).Returns(userId);

        var handler = new DeleteVideoCommandHandler(context, currentUser.Object);

        await handler.Handle(new DeleteVideoCommand(video.VideoId), CancellationToken.None);

        var updated = await context.Videos.FindAsync(video.VideoId);
        updated!.IsDeleted.Should().BeTrue();
    }
}
