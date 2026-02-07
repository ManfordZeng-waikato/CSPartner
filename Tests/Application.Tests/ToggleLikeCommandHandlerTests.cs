using Application.Common.Interfaces;
using Application.Features.Videos.Commands.ToggleLike;
using Application.Tests.Helpers;
using Domain.Exceptions;
using Domain.Videos;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace Application.Tests;

public class ToggleLikeCommandHandlerTests
{
    [Fact]
    public async Task Handle_throws_when_video_not_found()
    {
        using var scope = TestDbContextScope.Create();
        var context = scope.Context;

        var userId = Guid.NewGuid();
        var currentUser = new Mock<ICurrentUserService>();
        currentUser.SetupGet(c => c.UserId).Returns(userId);

        var handler = new ToggleLikeCommandHandler(context, currentUser.Object);

        var act = async () => await handler.Handle(new ToggleLikeCommand(Guid.NewGuid()), CancellationToken.None);

        await act.Should().ThrowAsync<VideoNotFoundException>();
    }

    [Fact]
    public async Task Handle_adds_like_when_none_exists()
    {
        using var scope = TestDbContextScope.Create();
        var context = scope.Context;

        var userId = Guid.NewGuid();
        var video = new HighlightVideo(userId, "t", "url");
        context.Videos.Add(video);
        await context.SaveChangesAsync();
        var videoId = video.VideoId;

        var currentUser = new Mock<ICurrentUserService>();
        currentUser.SetupGet(c => c.UserId).Returns(userId);

        var handler = new ToggleLikeCommandHandler(context, currentUser.Object);

        var result = await handler.Handle(new ToggleLikeCommand(videoId), CancellationToken.None);

        result.Should().BeTrue();
        context.ChangeTracker.Entries<VideoLike>().Should().ContainSingle(e => e.State == EntityState.Added);
        video.LikeCount.Should().Be(1);
    }

    [Fact]
    public async Task Handle_removes_like_when_exists()
    {
        using var scope = TestDbContextScope.Create();
        var context = scope.Context;

        var userId = Guid.NewGuid();
        var video = new HighlightVideo(userId, "t", "url");
        video.ApplyLikeAdded();
        context.Videos.Add(video);
        var videoId = video.VideoId;
        var like = new VideoLike(video.VideoId, userId);
        context.VideoLikes.Add(like);
        await context.SaveChangesAsync();

        var currentUser = new Mock<ICurrentUserService>();
        currentUser.SetupGet(c => c.UserId).Returns(userId);

        var handler = new ToggleLikeCommandHandler(context, currentUser.Object);

        var result = await handler.Handle(new ToggleLikeCommand(videoId), CancellationToken.None);

        result.Should().BeTrue();
        context.ChangeTracker.Entries<VideoLike>().Should().ContainSingle(e => e.State == EntityState.Deleted);
        video.LikeCount.Should().Be(0);
    }
}
