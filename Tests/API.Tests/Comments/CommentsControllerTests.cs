using System.Net;
using API.Tests.Helpers;
using Domain.Comments;
using Domain.Videos;
using FluentAssertions;
using Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace API.Tests.Comments;

public class CommentsControllerTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;

    public CommentsControllerTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task DeleteComment_returns_no_content_and_soft_deletes_comment()
    {
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            await db.Database.EnsureDeletedAsync();
            await db.Database.EnsureCreatedAsync();

            var video = new HighlightVideo(TestAuthDefaults.UserId, "t", "url");
            db.Videos.Add(video);

            var comment = new Comment(video.VideoId, TestAuthDefaults.UserId, "hello");
            db.Comments.Add(comment);
            await db.SaveChangesAsync();
        }

        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Add("X-Test-Auth", "true");

        Guid commentId;
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            commentId = await db.Comments.Select(c => c.CommentId).SingleAsync();
        }

        var response = await client.DeleteAsync($"/api/comments/{commentId}");

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var updated = await db.Comments.FindAsync(commentId);
            updated.Should().NotBeNull();
            updated!.IsDeleted.Should().BeTrue();
        }
    }

    [Fact]
    public async Task DeleteComment_returns_forbidden_when_not_owner()
    {
        Guid commentId;
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            await db.Database.EnsureDeletedAsync();
            await db.Database.EnsureCreatedAsync();

            var otherUserId = Guid.NewGuid();
            var video = new HighlightVideo(otherUserId, "t", "url");
            db.Videos.Add(video);

            var comment = new Comment(video.VideoId, otherUserId, "hello");
            db.Comments.Add(comment);
            await db.SaveChangesAsync();

            commentId = comment.CommentId;
        }

        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Add("X-Test-Auth", "true");

        var response = await client.DeleteAsync($"/api/comments/{commentId}");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task DeleteComment_returns_not_found_when_missing()
    {
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            await db.Database.EnsureDeletedAsync();
            await db.Database.EnsureCreatedAsync();
        }

        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Add("X-Test-Auth", "true");

        var response = await client.DeleteAsync($"/api/comments/{Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DeleteComment_soft_deletes_child_comments()
    {
        Guid parentId;
        Guid childId;
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            await db.Database.EnsureDeletedAsync();
            await db.Database.EnsureCreatedAsync();

            var video = new HighlightVideo(TestAuthDefaults.UserId, "t", "url");
            db.Videos.Add(video);
            await db.SaveChangesAsync();

            var parent = new Comment(video.VideoId, TestAuthDefaults.UserId, "parent");
            db.Comments.Add(parent);
            await db.SaveChangesAsync();
            parentId = parent.CommentId;

            var child = new Comment(video.VideoId, TestAuthDefaults.UserId, "child", parentId);
            db.Comments.Add(child);
            await db.SaveChangesAsync();
            childId = child.CommentId;
        }

        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Add("X-Test-Auth", "true");

        var response = await client.DeleteAsync($"/api/comments/{parentId}");

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var updatedParent = await db.Comments.FindAsync(parentId);
            var updatedChild = await db.Comments.FindAsync(childId);
            updatedParent!.IsDeleted.Should().BeTrue();
            updatedChild!.IsDeleted.Should().BeTrue();
        }
    }

    [Fact]
    public async Task DeleteComment_decrements_video_comment_count_for_tree()
    {
        Guid parentId;
        Guid childId;
        Guid videoId;
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            await db.Database.EnsureDeletedAsync();
            await db.Database.EnsureCreatedAsync();

            var video = new HighlightVideo(TestAuthDefaults.UserId, "t", "url");
            db.Videos.Add(video);
            await db.SaveChangesAsync();
            videoId = video.VideoId;

            var parent = new Comment(videoId, TestAuthDefaults.UserId, "parent");
            var child = new Comment(videoId, TestAuthDefaults.UserId, "child", parent.CommentId);
            db.Comments.AddRange(parent, child);
            video.ApplyCommentAdded();
            video.ApplyCommentAdded();
            await db.SaveChangesAsync();
            parentId = parent.CommentId;
            childId = child.CommentId;
        }

        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Add("X-Test-Auth", "true");

        var response = await client.DeleteAsync($"/api/comments/{parentId}");

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var updatedVideo = await db.Videos.FindAsync(videoId);
            updatedVideo!.CommentCount.Should().Be(0);

            var updatedChild = await db.Comments.FindAsync(childId);
            updatedChild!.IsDeleted.Should().BeTrue();
        }
    }

    [Fact]
    public async Task DeleteComment_returns_not_found_when_already_deleted()
    {
        Guid commentId;
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            await db.Database.EnsureDeletedAsync();
            await db.Database.EnsureCreatedAsync();

            var video = new HighlightVideo(TestAuthDefaults.UserId, "t", "url");
            db.Videos.Add(video);
            await db.SaveChangesAsync();

            var comment = new Comment(video.VideoId, TestAuthDefaults.UserId, "hello");
            comment.SoftDelete();
            db.Comments.Add(comment);
            await db.SaveChangesAsync();
            commentId = comment.CommentId;
        }

        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Add("X-Test-Auth", "true");

        var response = await client.DeleteAsync($"/api/comments/{commentId}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DeleteComment_returns_unauthorized_when_not_authenticated()
    {
        Guid commentId;
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            await db.Database.EnsureDeletedAsync();
            await db.Database.EnsureCreatedAsync();

            var video = new HighlightVideo(TestAuthDefaults.UserId, "t", "url");
            db.Videos.Add(video);

            var comment = new Comment(video.VideoId, TestAuthDefaults.UserId, "hello");
            db.Comments.Add(comment);
            await db.SaveChangesAsync();

            commentId = comment.CommentId;
        }

        var client = _factory.CreateClient();

        var response = await client.DeleteAsync($"/api/comments/{commentId}");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
