using Application.Common.Interfaces;
using Application.DTOs.Ai;
using Application.Features.Videos.Commands.GenerateVideoAiMeta;
using Application.Tests.Helpers;
using Domain.Ai;
using Domain.Exceptions;
using Domain.Videos;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;

namespace Application.Tests;

public class GenerateVideoAiMetaCommandHandlerTests
{
    [Fact]
    public async Task Handle_throws_when_video_missing()
    {
        using var scope = TestDbContextScope.Create();
        var context = scope.Context;

        var ai = new Mock<IAiVideoService>();
        var currentUser = new Mock<ICurrentUserService>();
        currentUser.SetupGet(c => c.UserId).Returns(Guid.NewGuid());
        var logger = new Mock<ILogger<GenerateVideoAiMetaCommandHandler>>();

        var handler = new GenerateVideoAiMetaCommandHandler(context, ai.Object, currentUser.Object, logger.Object);

        var act = async () => await handler.Handle(
            new GenerateVideoAiMetaCommand(Guid.NewGuid(), "Map", "Weapon", HighlightType.Clutch),
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

        var ai = new Mock<IAiVideoService>();
        var currentUser = new Mock<ICurrentUserService>();
        currentUser.SetupGet(c => c.UserId).Returns(Guid.NewGuid());
        var logger = new Mock<ILogger<GenerateVideoAiMetaCommandHandler>>();

        var handler = new GenerateVideoAiMetaCommandHandler(context, ai.Object, currentUser.Object, logger.Object);

        var act = async () => await handler.Handle(
            new GenerateVideoAiMetaCommand(video.VideoId, "Map", "Weapon", HighlightType.Clutch),
            CancellationToken.None);

        await act.Should().ThrowAsync<UnauthorizedOperationException>();
    }

    [Fact]
    public async Task Handle_completes_when_background_user()
    {
        using var scope = TestDbContextScope.Create();
        var context = scope.Context;

        var userId = Guid.NewGuid();
        var video = new HighlightVideo(userId, "t", "url");
        context.Videos.Add(video);
        await context.SaveChangesAsync();

        var ai = new Mock<IAiVideoService>();
        ai.Setup(s => s.GenerateVideoMetaAsync(It.IsAny<VideoAiInputDto>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new VideoAiResultDto("AI desc"));

        var currentUser = new Mock<ICurrentUserService>();
        currentUser.SetupGet(c => c.UserId).Returns((Guid?)null);

        var logger = new Mock<ILogger<GenerateVideoAiMetaCommandHandler>>();

        var handler = new GenerateVideoAiMetaCommandHandler(context, ai.Object, currentUser.Object, logger.Object);

        var result = await handler.Handle(
            new GenerateVideoAiMetaCommand(video.VideoId, "Map", "Weapon", HighlightType.Clutch),
            CancellationToken.None);

        result.Description.Should().Be("AI desc");
        var updated = await context.Videos.FindAsync(video.VideoId);
        updated!.AiStatus.Should().Be(AiStatus.Completed);
        updated.AiDescription.Should().Be("AI desc");
    }

    [Fact]
    public async Task Handle_marks_failed_when_quota_exceeded()
    {
        using var scope = TestDbContextScope.Create();
        var context = scope.Context;

        var userId = Guid.NewGuid();
        var video = new HighlightVideo(userId, "t", "url");
        context.Videos.Add(video);
        await context.SaveChangesAsync();

        var ai = new Mock<IAiVideoService>();
        ai.Setup(s => s.GenerateVideoMetaAsync(It.IsAny<VideoAiInputDto>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new AiServiceQuotaExceededException("quota"));

        var currentUser = new Mock<ICurrentUserService>();
        currentUser.SetupGet(c => c.UserId).Returns(userId);

        var logger = new Mock<ILogger<GenerateVideoAiMetaCommandHandler>>();

        var handler = new GenerateVideoAiMetaCommandHandler(context, ai.Object, currentUser.Object, logger.Object);

        var act = async () => await handler.Handle(
            new GenerateVideoAiMetaCommand(video.VideoId, "Map", "Weapon", HighlightType.Clutch),
            CancellationToken.None);

        await act.Should().ThrowAsync<AiServiceQuotaExceededException>();

        var updated = await context.Videos.FindAsync(video.VideoId);
        updated!.AiStatus.Should().Be(AiStatus.Failed);
        updated.AiLastError.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task Handle_marks_failed_when_ai_error()
    {
        using var scope = TestDbContextScope.Create();
        var context = scope.Context;

        var userId = Guid.NewGuid();
        var video = new HighlightVideo(userId, "t", "url");
        context.Videos.Add(video);
        await context.SaveChangesAsync();

        var ai = new Mock<IAiVideoService>();
        ai.Setup(s => s.GenerateVideoMetaAsync(It.IsAny<VideoAiInputDto>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new AiServiceException("ai error", 500));

        var currentUser = new Mock<ICurrentUserService>();
        currentUser.SetupGet(c => c.UserId).Returns(userId);

        var logger = new Mock<ILogger<GenerateVideoAiMetaCommandHandler>>();

        var handler = new GenerateVideoAiMetaCommandHandler(context, ai.Object, currentUser.Object, logger.Object);

        var act = async () => await handler.Handle(
            new GenerateVideoAiMetaCommand(video.VideoId, "Map", "Weapon", HighlightType.Clutch),
            CancellationToken.None);

        await act.Should().ThrowAsync<AiServiceException>();

        var updated = await context.Videos.FindAsync(video.VideoId);
        updated!.AiStatus.Should().Be(AiStatus.Failed);
        updated.AiLastError.Should().NotBeNullOrWhiteSpace();
    }
}
