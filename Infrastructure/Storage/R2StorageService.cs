using Amazon.S3;
using Amazon.S3.Model;
using Application.Interfaces.Services;
using Application.DTOs.Storage;
using System.Linq;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Storage;

public class R2StorageService : IStorageService, IDisposable
{
    private readonly IAmazonS3 _s3Client;
    private readonly string _bucketName;
    private readonly string _publicUrlTemplate;
    private readonly string _accountId;
    private readonly string _s3ServiceUrl;
    private readonly ILogger<R2StorageService> _logger;

    public R2StorageService(IConfiguration configuration, ILogger<R2StorageService> logger)
    {
        _logger = logger;
        _accountId = configuration["CloudflareR2:AccountId"] ?? throw new InvalidOperationException("CloudflareR2:AccountId is not configured");
        var accessKeyId = configuration["CloudflareR2:AccessKeyId"] ?? throw new InvalidOperationException("CloudflareR2:AccessKeyId is not configured");
        var secretAccessKey = configuration["CloudflareR2:SecretAccessKey"] ?? throw new InvalidOperationException("CloudflareR2:SecretAccessKey is not configured");
        _bucketName = configuration["CloudflareR2:BucketName"] ?? throw new InvalidOperationException("CloudflareR2:BucketName is not configured");
        _publicUrlTemplate = configuration["CloudflareR2:PublicUrl"] ?? $"https://pub-{_accountId}.r2.dev";

        // Prefer configured S3ServiceUrl, otherwise build from AccountId
        _s3ServiceUrl = configuration["CloudflareR2:S3ServiceUrl"] 
            ?? $"https://{_accountId}.r2.cloudflarestorage.com";

        if (string.IsNullOrWhiteSpace(_s3ServiceUrl))
        {
            throw new InvalidOperationException("CloudflareR2:S3ServiceUrl is not configured and cannot be built from AccountId");
        }

        var config = new AmazonS3Config
        {
            ServiceURL = _s3ServiceUrl,
            ForcePathStyle = true,
            UseHttp = false,
            AllowAutoRedirect = false, // Disable auto-redirect, we need explicit handling
            DisableHostPrefixInjection = true // Disable host prefix injection
        };


        _s3Client = new AmazonS3Client(accessKeyId, secretAccessKey, config);
        
        _logger.LogInformation("R2StorageService initialized. AccountId: {AccountId}, Bucket: {BucketName}, ServiceURL: {ServiceURL}", 
            _accountId, _bucketName, config.ServiceURL);
    }

    public string GetPublicUrl(string objectKey)
    {
        var baseUrl = _publicUrlTemplate.Replace("{accountId}", _accountId).TrimEnd('/');
        return $"{baseUrl}/{objectKey.TrimStart('/')}";
    }

    public Task<PreSignedUploadResult> GetVideoUploadUrlAsync(string fileName, string contentType, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ValidateVideoFile(fileName);

        var objectKey = GenerateVideoKey(fileName);
        var resolvedContentType = string.IsNullOrWhiteSpace(contentType)
            ? GetVideoContentType(fileName)
            : contentType;

        var expiresAtUtc = DateTime.UtcNow.AddMinutes(15);
        var request = new GetPreSignedUrlRequest
        {
            BucketName = _bucketName,
            Key = objectKey,
            Verb = HttpVerb.PUT,
            Expires = expiresAtUtc,
            ContentType = resolvedContentType
        };

        var uploadUrl = _s3Client.GetPreSignedURL(request);
        var publicUrl = GetPublicUrl(objectKey);

        _logger.LogInformation(
            "Generated pre-signed upload URL for R2. ObjectKey: {ObjectKey}, ExpiresAtUtc: {ExpiresAtUtc}, ContentType: {ContentType}",
            objectKey,
            expiresAtUtc,
            resolvedContentType);

        return Task.FromResult(new PreSignedUploadResult(
            uploadUrl,
            objectKey,
            publicUrl,
            expiresAtUtc,
            resolvedContentType));
    }

    private static string GenerateVideoKey(string fileName)
    {
        var extension = Path.GetExtension(fileName);
        var uniqueId = Guid.NewGuid().ToString("N");
        var timestamp = DateTime.UtcNow.ToString("yyyyMMdd");
        // No need to include bucket name as it's already specified in BucketName parameter
        return $"{timestamp}/highlight-{uniqueId}{extension}";
    }

    private static string GetVideoContentType(string fileName)
    {
        var extension = Path.GetExtension(fileName).ToLowerInvariant();
        return extension switch
        {
            ".mp4" => "video/mp4",
            ".webm" => "video/webm",
            ".mov" => "video/quicktime",
            ".avi" => "video/x-msvideo",
            _ => "video/mp4" // Default value
        };
    }

    public async Task<bool> FileExistsAsync(string objectKey, CancellationToken cancellationToken = default)
    {
        try
        {
            var request = new GetObjectMetadataRequest
            {
                BucketName = _bucketName,
                Key = objectKey
            };

            await _s3Client.GetObjectMetadataAsync(request, cancellationToken);
            return true;
        }
        catch (Amazon.S3.AmazonS3Exception ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error checking file existence for object key: {ObjectKey}", objectKey);
            // Return false on error to be safe - we don't want to create records for non-existent files
            return false;
        }
    }

    private static void ValidateVideoFile(string fileName)
    {
        var allowedVideoExtensions = new[] { ".mp4", ".webm", ".mov", ".avi" };
        var fileExtension = Path.GetExtension(fileName).ToLowerInvariant();
        if (!allowedVideoExtensions.Contains(fileExtension))
        {
            throw new ArgumentException("Unsupported video format. Supported: mp4, webm, mov, avi", nameof(fileName));
        }
    }

    public void Dispose()
    {
        _s3Client?.Dispose();
    }
}
