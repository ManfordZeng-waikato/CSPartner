using Application.DTOs.Storage;
using FluentAssertions;
using Infrastructure.Storage;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;

namespace Infrastructure.Tests.Storage;

public class R2StorageServiceTests
{
    private static R2StorageService CreateService(Dictionary<string, string?>? overrides = null)
    {
        var settings = new Dictionary<string, string?>
        {
            ["CloudflareR2:AccountId"] = "acc",
            ["CloudflareR2:AccessKeyId"] = "key",
            ["CloudflareR2:SecretAccessKey"] = "secret",
            ["CloudflareR2:BucketName"] = "bucket",
            ["CloudflareR2:PublicUrl"] = "https://pub-{accountId}.r2.dev",
            ["CloudflareR2:S3ServiceUrl"] = "https://acc.r2.cloudflarestorage.com"
        };
        if (overrides != null)
        {
            foreach (var kv in overrides)
            {
                settings[kv.Key] = kv.Value;
            }
        }

        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(settings)
            .Build();

        return new R2StorageService(config, NullLogger<R2StorageService>.Instance);
    }

    [Fact]
    public void GetPublicUrl_builds_url_from_template()
    {
        using var service = CreateService();

        var url = service.GetPublicUrl("/videos/test.mp4");

        url.Should().Be("https://pub-acc.r2.dev/videos/test.mp4");
    }

    [Fact]
    public async Task GetVideoUploadUrlAsync_uses_inferred_content_type_when_invalid()
    {
        using var service = CreateService();

        var result = await service.GetVideoUploadUrlAsync(Guid.NewGuid(), "clip.mp4", "text/plain");

        result.ContentType.Should().Be("video/mp4");
        result.ObjectKey.Should().Contain("/highlight-");
        result.ObjectKey.Should().EndWith(".mp4");
        result.UploadUrl.Should().NotBeNullOrWhiteSpace();
        result.PublicUrl.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task GetVideoUploadUrlAsync_throws_for_unsupported_extension()
    {
        using var service = CreateService();

        var act = async () => await service.GetVideoUploadUrlAsync(Guid.NewGuid(), "clip.txt", "text/plain");

        await act.Should().ThrowAsync<ArgumentException>();
    }
}
