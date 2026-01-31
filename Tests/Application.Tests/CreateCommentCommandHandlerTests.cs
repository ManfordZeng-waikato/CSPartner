using Application.Common.Interfaces;
using Application.Features.Comments.Commands.CreateComment;
using Application.Tests.Helpers;
using Domain.Comments;
using Domain.Exceptions;
using Domain.Videos;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace Application.Tests;

public class CreateCommentCommandHandlerTests
{
    [Fact]
    public async Task Handle_throws_when_user_not_authenticated()
    {
        using var scope = TestDbContextScope.Create();
        var context = scope.Context;

        var currentUser = new Mock<ICurrentUserService>();
        currentUser.SetupGet(c => c.UserId).Returns((Guid?)null);

        var handler = new CreateCommentCommandHandler(context, currentUser.Object);

        var act = async () => await handler.Handle(
            new CreateCommentCommand(Guid.NewGuid(), "hi"),
            CancellationToken.None);

        await act.Should().ThrowAsync<AuthenticationRequiredException>();
    }

    [Fact]
    public async Task Handle_throws_when_video_not_found()
    {
        using var scope = TestDbContextScope.Create();
        var context = scope.Context;

        var currentUser = new Mock<ICurrentUserService>();
        currentUser.SetupGet(c => c.UserId).Returns(Guid.NewGuid());

        var handler = new CreateCommentCommandHandler(context, currentUser.Object);

        var act = async () => await handler.Handle(
            new CreateCommentCommand(Guid.NewGuid(), "hi"),
            CancellationToken.None);

        await act.Should().ThrowAsync<VideoNotFoundException>();
    }

    [Fact]
    public async Task Handle_throws_when_parent_comment_missing()
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

        var handler = new CreateCommentCommandHandler(context, currentUser.Object);

        var act = async () => await handler.Handle(
            new CreateCommentCommand(videoId, "hi", Guid.NewGuid()),
            CancellationToken.None);

        await act.Should().ThrowAsync<CommentNotFoundException>();
    }

    [Fact]
    public async Task Handle_creates_comment_and_increments_count()
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

        var handler = new CreateCommentCommandHandler(context, currentUser.Object);

        var result = await handler.Handle(new CreateCommentCommand(videoId, "hello"), CancellationToken.None);

        context.ChangeTracker.Entries<Comment>().Should().ContainSingle(e => e.State == EntityState.Added);
        video.CommentCount.Should().Be(1);
        result.Content.Should().Be("hello");
    }
}
