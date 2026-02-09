using System.Net;
using System.Net.Http.Json;
using API.Tests.Helpers;
using Application.DTOs.UserProfile;
using Domain.Users;
using Domain.Videos;
using FluentAssertions;
using Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace API.Tests.UserProfiles;

public class UserProfilesControllerTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;

    public UserProfilesControllerTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task GetUserProfile_returns_profile()
    {
        var userId = Guid.NewGuid();
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            await db.Database.EnsureDeletedAsync();
            await db.Database.EnsureCreatedAsync();

            var profile = new UserProfile(userId);
            profile.Update("User", "Bio", null, null, null);
            db.UserProfiles.Add(profile);

            var video = new HighlightVideo(userId, "pub", "url1");
            db.Videos.Add(video);

            await db.SaveChangesAsync();
        }

        var client = _factory.CreateClient();

        var result = await client.GetFromJsonAsync<UserProfileDto>($"/api/userprofiles/{userId}", TestJsonOptions.Default);

        result.Should().NotBeNull();
        result!.UserId.Should().Be(userId);
        result.Videos.Should().HaveCount(1);
    }

    [Fact]
    public async Task GetUserProfile_returns_not_found_when_missing()
    {
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            await db.Database.EnsureDeletedAsync();
            await db.Database.EnsureCreatedAsync();
        }

        var client = _factory.CreateClient();

        var response = await client.GetAsync($"/api/userprofiles/{Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task UpdateUserProfile_returns_forbidden_when_user_mismatch()
    {
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Add("X-Test-Auth", "true");

        var otherUserId = Guid.NewGuid();
        var response = await client.PutAsJsonAsync($"/api/userprofiles/{otherUserId}", new UpdateUserProfileDto
        {
            DisplayName = "Nope"
        });

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task UpdateUserProfile_updates_when_user_matches()
    {
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            await db.Database.EnsureDeletedAsync();
            await db.Database.EnsureCreatedAsync();
        }

        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Add("X-Test-Auth", "true");

        var response = await client.PutAsJsonAsync($"/api/userprofiles/{TestAuthDefaults.UserId}", new UpdateUserProfileDto
        {
            DisplayName = "NewName",
            Bio = "Bio"
        });

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<UserProfileDto>(TestJsonOptions.Default);
        result.Should().NotBeNull();
        result!.DisplayName.Should().Be("NewName");
        result.Bio.Should().Be("Bio");
    }

    [Fact]
    public async Task UpdateUserProfile_returns_unauthorized_when_not_authenticated()
    {
        var client = _factory.CreateClient();

        var response = await client.PutAsJsonAsync($"/api/userprofiles/{Guid.NewGuid()}", new UpdateUserProfileDto
        {
            DisplayName = "Nope"
        });

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
