using System.Net;
using Amazon.S3;
using Amazon.S3.Model;
using Application.DTOs.Storage;
using FluentAssertions;
using Infrastructure.Storage;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace Infrastructure.Tests.Storage;

public class R2StorageServiceTests
{
    private static R2StorageService CreateService(Dictionary<string, string?>? overrides = null, IAmazonS3? s3Client = null)
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

        return new R2StorageService(config, NullLogger<R2StorageService>.Instance, s3Client);
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

    [Fact]
    public async Task FileExistsAsync_returns_true_when_metadata_found()
    {
        var s3 = new Mock<IAmazonS3>();
        s3.Setup(s => s.GetObjectMetadataAsync(It.IsAny<GetObjectMetadataRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new GetObjectMetadataResponse());

        using var service = CreateService(s3Client: s3.Object);

        var exists = await service.FileExistsAsync("videos/key.mp4");

        exists.Should().BeTrue();
    }

    [Fact]
    public async Task FileExistsAsync_returns_false_when_not_found()
    {
        var s3 = new Mock<IAmazonS3>();
        s3.Setup(s => s.GetObjectMetadataAsync(It.IsAny<GetObjectMetadataRequest>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new AmazonS3Exception("missing") { StatusCode = HttpStatusCode.NotFound });

        using var service = CreateService(s3Client: s3.Object);

        var exists = await service.FileExistsAsync("videos/missing.mp4");

        exists.Should().BeFalse();
    }

    [Fact]
    public async Task FileExistsAsync_returns_false_on_exception()
    {
        var s3 = new Mock<IAmazonS3>();
        s3.Setup(s => s.GetObjectMetadataAsync(It.IsAny<GetObjectMetadataRequest>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("boom"));

        using var service = CreateService(s3Client: s3.Object);

        var exists = await service.FileExistsAsync("videos/error.mp4");

        exists.Should().BeFalse();
    }
}
