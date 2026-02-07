using Application.Common.Interfaces;
using Application.Features.Videos.Commands.CreateVideo;
using Application.Tests.Helpers;
using Domain.Exceptions;
using Domain.Videos;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;

namespace Application.Tests;

public class CreateVideoCommandHandlerTests
{
    [Fact]
    public async Task Handle_throws_when_user_not_authenticated()
    {
        using var scope = TestDbContextScope.Create();
        var context = scope.Context;

        var currentUser = new Mock<ICurrentUserService>();
        currentUser.SetupGet(c => c.UserId).Returns((Guid?)null);

        var storage = new Mock<IStorageService>();
        var logger = new Mock<ILogger<CreateVideoCommandHandler>>();

        var handler = new CreateVideoCommandHandler(context, currentUser.Object, storage.Object, logger.Object);

        var act = async () => await handler.Handle(new CreateVideoCommand("videos/x/file.mp4"), CancellationToken.None);

        await act.Should().ThrowAsync<AuthenticationRequiredException>();
    }

    [Fact]
    public async Task Handle_throws_when_object_key_not_owned_by_user()
    {
        using var scope = TestDbContextScope.Create();
        var context = scope.Context;

        var userId = Guid.NewGuid();
        var currentUser = new Mock<ICurrentUserService>();
        currentUser.SetupGet(c => c.UserId).Returns(userId);

        var storage = new Mock<IStorageService>();
        var logger = new Mock<ILogger<CreateVideoCommandHandler>>();

        var handler = new CreateVideoCommandHandler(context, currentUser.Object, storage.Object, logger.Object);

        var act = async () => await handler.Handle(
            new CreateVideoCommand("videos/other-user/file.mp4"),
            CancellationToken.None);

        await act.Should().ThrowAsync<InvalidObjectKeyException>();
    }

    [Fact]
    public async Task Handle_throws_when_video_file_missing()
    {
        using var scope = TestDbContextScope.Create();
        var context = scope.Context;

        var userId = Guid.NewGuid();
        var currentUser = new Mock<ICurrentUserService>();
        currentUser.SetupGet(c => c.UserId).Returns(userId);

        var storage = new Mock<IStorageService>();
        storage.Setup(s => s.FileExistsAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var logger = new Mock<ILogger<CreateVideoCommandHandler>>();

        var handler = new CreateVideoCommandHandler(context, currentUser.Object, storage.Object, logger.Object);
        var key = $"videos/{userId}/20250101/highlight.mp4";

        var act = async () => await handler.Handle(new CreateVideoCommand(key), CancellationToken.None);

        await act.Should().ThrowAsync<StorageFileNotFoundException>();
    }

    [Fact]
    public async Task Handle_throws_when_thumbnail_object_key_not_owned_by_user()
    {
        using var scope = TestDbContextScope.Create();
        var context = scope.Context;

        var userId = Guid.NewGuid();
        var currentUser = new Mock<ICurrentUserService>();
        currentUser.SetupGet(c => c.UserId).Returns(userId);

        var storage = new Mock<IStorageService>();
        storage.Setup(s => s.FileExistsAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var logger = new Mock<ILogger<CreateVideoCommandHandler>>();

        var handler = new CreateVideoCommandHandler(context, currentUser.Object, storage.Object, logger.Object);
        var key = $"videos/{userId}/20250101/highlight.mp4";

        var act = async () => await handler.Handle(
            new CreateVideoCommand(
                key,
                "videos/other-user/thumb.jpg"),
            CancellationToken.None);

        await act.Should().ThrowAsync<InvalidObjectKeyException>();
    }

    [Fact]
    public async Task Handle_creates_video_even_if_thumbnail_missing()
    {
        using var scope = TestDbContextScope.Create();
        var context = scope.Context;

        var userId = Guid.NewGuid();
        var currentUser = new Mock<ICurrentUserService>();
        currentUser.SetupGet(c => c.UserId).Returns(userId);

        var key = $"videos/{userId}/20250101/highlight.mp4";
        var thumbKey = $"videos/{userId}/20250101/thumb.jpg";

        var storage = new Mock<IStorageService>();
        storage.Setup(s => s.FileExistsAsync(key, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        storage.Setup(s => s.FileExistsAsync(thumbKey, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        storage.Setup(s => s.GetPublicUrl(It.IsAny<string>()))
            .Returns<string>(objectKey => $"https://cdn/{objectKey}");

        var logger = new Mock<ILogger<CreateVideoCommandHandler>>();

        var handler = new CreateVideoCommandHandler(context, currentUser.Object, storage.Object, logger.Object);

        var result = await handler.Handle(
            new CreateVideoCommand(key, thumbKey, "Title"),
            CancellationToken.None);

        var video = await context.Videos.SingleAsync();
        video.ThumbnailUrl.Should().Be($"https://cdn/{thumbKey}");
        result.ThumbnailUrl.Should().Be($"https://cdn/{thumbKey}");
    }

    [Fact]
    public async Task Handle_creates_video_and_returns_dto()
    {
        using var scope = TestDbContextScope.Create();
        var context = scope.Context;

        var userId = Guid.NewGuid();
        var currentUser = new Mock<ICurrentUserService>();
        currentUser.SetupGet(c => c.UserId).Returns(userId);

        var storage = new Mock<IStorageService>();
        storage.Setup(s => s.FileExistsAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        storage.Setup(s => s.GetPublicUrl(It.IsAny<string>()))
            .Returns<string>(key => $"https://cdn/{key}");

        var logger = new Mock<ILogger<CreateVideoCommandHandler>>();

        var handler = new CreateVideoCommandHandler(context, currentUser.Object, storage.Object, logger.Object);
        var key = $"videos/{userId}/20250101/highlight.mp4";
        var thumbKey = $"videos/{userId}/20250101/thumb.jpg";

        var result = await handler.Handle(
            new CreateVideoCommand(
                key,
                thumbKey,
                "Title",
                "Desc",
                VideoVisibility.Private,
                "Mirage",
                "AK47",
                HighlightType.Clutch),
            CancellationToken.None);

        var video = await context.Videos.SingleAsync();
        video.VideoUrl.Should().Be($"https://cdn/{key}");
        video.ThumbnailUrl.Should().Be($"https://cdn/{thumbKey}");
        video.Visibility.Should().Be(VideoVisibility.Private);
        video.TagsJson.Should().Contain("Mirage");
        video.TagsJson.Should().Contain("AK47");
        video.TagsJson.Should().Contain(nameof(HighlightType.Clutch));
        result.VideoUrl.Should().Be($"https://cdn/{key}");
        result.Title.Should().Be("Title");
        result.Visibility.Should().Be(VideoVisibility.Private);
    }
}
