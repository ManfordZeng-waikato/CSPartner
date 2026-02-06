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
}
