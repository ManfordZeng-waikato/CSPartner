using System.Net;
using System.Net.Http.Json;
using API.Tests.Helpers;
using Application.DTOs.Common;
using Application.DTOs.Comment;
using Application.DTOs.Video;
using Domain.Videos;
using FluentAssertions;
using Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace API.Tests.Videos;

public class VideosControllerTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;

    public VideosControllerTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task GetVideos_returns_public_and_own_private()
    {
        Guid publicOtherId;
        Guid privateOtherId;
        Guid privateOwnId;
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            await db.Database.EnsureDeletedAsync();
            await db.Database.EnsureCreatedAsync();

            var otherUserId = Guid.NewGuid();
            var publicOther = new HighlightVideo(otherUserId, "pub", "url1");
            var privateOther = new HighlightVideo(otherUserId, "priv", "url2");
            privateOther.SetVisibility(VideoVisibility.Private);

            var privateOwn = new HighlightVideo(TestAuthDefaults.UserId, "mine", "url3");
            privateOwn.SetVisibility(VideoVisibility.Private);

            db.Videos.AddRange(publicOther, privateOther, privateOwn);
            await db.SaveChangesAsync();

            publicOtherId = publicOther.VideoId;
            privateOtherId = privateOther.VideoId;
            privateOwnId = privateOwn.VideoId;
        }

        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Add("X-Test-Auth", "true");

        var result = await client.GetFromJsonAsync<CursorPagedResult<VideoDto>>("/api/videos", TestJsonOptions.Default);

        result.Should().NotBeNull();
        var ids = result!.Items.Select(v => v.VideoId).ToList();
        ids.Should().Contain(publicOtherId);
        ids.Should().Contain(privateOwnId);
        ids.Should().NotContain(privateOtherId);
    }

    [Fact]
    public async Task GetVideo_returns_not_found_for_private_not_owner()
    {
        Guid privateOtherId;
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            await db.Database.EnsureDeletedAsync();
            await db.Database.EnsureCreatedAsync();

            var otherUserId = Guid.NewGuid();
            var privateOther = new HighlightVideo(otherUserId, "priv", "url2");
            privateOther.SetVisibility(VideoVisibility.Private);
            db.Videos.Add(privateOther);
            await db.SaveChangesAsync();

            privateOtherId = privateOther.VideoId;
        }

        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Add("X-Test-Auth", "true");

        var response = await client.GetAsync($"/api/videos/{privateOtherId}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetVideosByUser_returns_only_public_for_other_user()
    {
        Guid otherUserId;
        Guid publicId;
        Guid privateId;
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            await db.Database.EnsureDeletedAsync();
            await db.Database.EnsureCreatedAsync();

            otherUserId = Guid.NewGuid();
            var publicVideo = new HighlightVideo(otherUserId, "pub", "url1");
            var privateVideo = new HighlightVideo(otherUserId, "priv", "url2");
            privateVideo.SetVisibility(VideoVisibility.Private);

            db.Videos.AddRange(publicVideo, privateVideo);
            await db.SaveChangesAsync();

            publicId = publicVideo.VideoId;
            privateId = privateVideo.VideoId;
        }

        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Add("X-Test-Auth", "true");

        var result = await client.GetFromJsonAsync<List<VideoDto>>($"/api/videos/user/{otherUserId}", TestJsonOptions.Default);

        result.Should().NotBeNull();
        result!.Select(v => v.VideoId).Should().Contain(publicId);
        result.Select(v => v.VideoId).Should().NotContain(privateId);
    }

    [Fact]
    public async Task GetUploadUrl_returns_bad_request_when_filename_missing()
    {
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Add("X-Test-Auth", "true");

        var response = await client.PostAsJsonAsync("/api/videos/upload-url", new
        {
            fileName = "",
            contentType = "video/mp4"
        });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task UpdateVideo_returns_no_content_for_owner()
    {
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
        }

        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Add("X-Test-Auth", "true");

        var response = await client.PutAsJsonAsync($"/api/videos/{videoId}", new
        {
            visibility = VideoVisibility.Private
        });

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var updated = await db.Videos.FindAsync(videoId);
            updated!.Visibility.Should().Be(VideoVisibility.Private);
        }
    }

    [Fact]
    public async Task UpdateVideo_returns_forbidden_for_non_owner()
    {
        Guid videoId;
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            await db.Database.EnsureDeletedAsync();
            await db.Database.EnsureCreatedAsync();

            var otherUserId = Guid.NewGuid();
            var video = new HighlightVideo(otherUserId, "t", "url");
            db.Videos.Add(video);
            await db.SaveChangesAsync();
            videoId = video.VideoId;
        }

        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Add("X-Test-Auth", "true");

        var response = await client.PutAsJsonAsync($"/api/videos/{videoId}", new
        {
            visibility = VideoVisibility.Private
        });

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task UpdateVideo_returns_not_found_when_missing()
    {
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            await db.Database.EnsureDeletedAsync();
            await db.Database.EnsureCreatedAsync();
        }

        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Add("X-Test-Auth", "true");

        var response = await client.PutAsJsonAsync($"/api/videos/{Guid.NewGuid()}", new
        {
            visibility = VideoVisibility.Private
        });

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DeleteVideo_soft_deletes_when_owner()
    {
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
        }

        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Add("X-Test-Auth", "true");

        var response = await client.DeleteAsync($"/api/videos/{videoId}");

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var updated = await db.Videos.FindAsync(videoId);
            updated!.IsDeleted.Should().BeTrue();
        }
    }

    [Fact]
    public async Task DeleteVideo_returns_forbidden_when_non_owner()
    {
        Guid videoId;
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            await db.Database.EnsureDeletedAsync();
            await db.Database.EnsureCreatedAsync();

            var otherUserId = Guid.NewGuid();
            var video = new HighlightVideo(otherUserId, "t", "url");
            db.Videos.Add(video);
            await db.SaveChangesAsync();
            videoId = video.VideoId;
        }

        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Add("X-Test-Auth", "true");

        var response = await client.DeleteAsync($"/api/videos/{videoId}");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task DeleteVideo_returns_not_found_when_missing()
    {
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            await db.Database.EnsureDeletedAsync();
            await db.Database.EnsureCreatedAsync();
        }

        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Add("X-Test-Auth", "true");

        var response = await client.DeleteAsync($"/api/videos/{Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task ToggleLike_increments_like_count()
    {
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
        }

        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Add("X-Test-Auth", "true");

        var response = await client.PostAsync($"/api/videos/{videoId}/like", content: null);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var updated = await db.Videos.FindAsync(videoId);
            updated!.LikeCount.Should().Be(1);
        }
    }

    [Fact]
    public async Task ToggleLike_returns_not_found_when_video_missing()
    {
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            await db.Database.EnsureDeletedAsync();
            await db.Database.EnsureCreatedAsync();
        }

        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Add("X-Test-Auth", "true");

        var response = await client.PostAsync($"/api/videos/{Guid.NewGuid()}/like", content: null);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task ToggleLike_returns_unauthorized_when_not_authenticated()
    {
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            await db.Database.EnsureDeletedAsync();
            await db.Database.EnsureCreatedAsync();
        }

        var client = _factory.CreateClient();

        var response = await client.PostAsync($"/api/videos/{Guid.NewGuid()}/like", content: null);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetVideo_increments_view_count()
    {
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
        }

        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Add("X-Test-Auth", "true");

        var response = await client.GetAsync($"/api/videos/{videoId}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var updated = await db.Videos.FindAsync(videoId);
            updated!.ViewCount.Should().Be(1);
        }
    }
    [Fact]
    public async Task CreateComment_returns_created_and_persists_comment()
    {
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
        }

        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Add("X-Test-Auth", "true");

        var response = await client.PostAsJsonAsync($"/api/videos/{videoId}/comments", new CreateCommentDto
        {
            Content = "hello"
        });

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var dto = await response.Content.ReadFromJsonAsync<CommentDto>(TestJsonOptions.Default);
        dto.Should().NotBeNull();
        dto!.Content.Should().Be("hello");

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            (await db.Comments.CountAsync()).Should().Be(1);
        }
    }

    [Fact]
    public async Task CreateComment_returns_unauthorized_when_not_authenticated()
    {
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
        }

        var client = _factory.CreateClient();

        var response = await client.PostAsJsonAsync($"/api/videos/{videoId}/comments", new CreateCommentDto
        {
            Content = "hello"
        });

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task CreateComment_returns_bad_request_when_video_missing()
    {
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            await db.Database.EnsureDeletedAsync();
            await db.Database.EnsureCreatedAsync();
        }

        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Add("X-Test-Auth", "true");

        var response = await client.PostAsJsonAsync($"/api/videos/{Guid.NewGuid()}/comments", new CreateCommentDto
        {
            Content = "hello"
        });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CreateComment_returns_bad_request_when_parent_missing()
    {
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
        }

        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Add("X-Test-Auth", "true");

        var response = await client.PostAsJsonAsync($"/api/videos/{videoId}/comments", new CreateCommentDto
        {
            Content = "hello",
            ParentCommentId = Guid.NewGuid()
        });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetVideoComments_returns_list()
    {
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

            db.Comments.Add(new Domain.Comments.Comment(videoId, TestAuthDefaults.UserId, "hello"));
            await db.SaveChangesAsync();
        }

        var client = _factory.CreateClient();

        var result = await client.GetFromJsonAsync<List<CommentDto>>($"/api/videos/{videoId}/comments", TestJsonOptions.Default);

        result.Should().NotBeNull();
        result!.Should().HaveCount(1);
        result[0].Content.Should().Be("hello");
    }
}
