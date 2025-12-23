using Amazon.S3;
using Amazon.S3.Model;
using Application.Interfaces.Services;
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

    public async Task<string> UploadVideoAsync(Stream fileStream, string fileName, CancellationToken cancellationToken = default)
    {
        var objectKey = GenerateVideoKey(fileName);
        var contentType = GetVideoContentType(fileName);
        return await UploadFileAsync(fileStream, objectKey, contentType, cancellationToken);
    }

    public async Task<string> UploadThumbnailAsync(Stream fileStream, string fileName, CancellationToken cancellationToken = default)
    {
        var objectKey = GenerateThumbnailKey(fileName);
        var contentType = GetImageContentType(fileName);
        return await UploadFileAsync(fileStream, objectKey, contentType, cancellationToken);
    }

    public async Task<bool> DeleteFileAsync(string objectKey, CancellationToken cancellationToken = default)
    {
        try
        {
            var request = new DeleteObjectRequest
            {
                BucketName = _bucketName,
                Key = objectKey
            };

            await _s3Client.DeleteObjectAsync(request, cancellationToken);
            _logger.LogInformation("Deleted file from R2: {ObjectKey}", objectKey);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete file from R2: {ObjectKey}", objectKey);
            return false;
        }
    }

    public string GetPublicUrl(string objectKey)
    {
        var baseUrl = _publicUrlTemplate.Replace("{accountId}", _accountId).TrimEnd('/');
        return $"{baseUrl}/{objectKey.TrimStart('/')}";
    }

    private async Task<string> UploadFileAsync(Stream fileStream, string objectKey, string contentType, CancellationToken cancellationToken)
    {
        try
        {
            // Convert stream to byte array to avoid streaming signature issues
            byte[] fileBytes;
            using (var memoryStream = new MemoryStream())
            {
                await fileStream.CopyToAsync(memoryStream, cancellationToken);
                fileBytes = memoryStream.ToArray();
            }

            var request = new PutObjectRequest
            {
                BucketName = _bucketName,
                Key = objectKey,
                InputStream = new MemoryStream(fileBytes),
                ContentType = contentType,
                ServerSideEncryptionMethod = ServerSideEncryptionMethod.None,
                DisablePayloadSigning = true // Disable payload signing for R2 compatibility
            };

            await _s3Client.PutObjectAsync(request, cancellationToken);
            
            var publicUrl = GetPublicUrl(objectKey);
            _logger.LogInformation("Uploaded file to R2: {ObjectKey}, Public URL: {PublicUrl}, Size: {Size} bytes", 
                objectKey, publicUrl, fileBytes.Length);
            
            return publicUrl;
        }
        catch (Amazon.S3.AmazonS3Exception s3Ex)
        {
            _logger.LogError(s3Ex, "S3 Error uploading file to R2: {ObjectKey}, StatusCode: {StatusCode}, ErrorCode: {ErrorCode}, Message: {Message}", 
                objectKey, s3Ex.StatusCode, s3Ex.ErrorCode, s3Ex.Message);
            
            var errorMessage = s3Ex.ErrorCode switch
            {
                "AccessDenied" => "Access denied. Please check: 1) AccessKeyId and SecretAccessKey are correct 2) API Token has write permissions 3) Bucket name is correct",
                "NoSuchBucket" => $"Bucket '{_bucketName}' does not exist. Please check if the Bucket name is correct",
                "InvalidAccessKeyId" => "AccessKeyId is invalid. Please check if the configuration is correct",
                "SignatureDoesNotMatch" => "Signature mismatch. Please check if SecretAccessKey is correct",
                "PermanentRedirect" => $"Endpoint configuration error. Current S3ServiceUrl: {_s3ServiceUrl}. Error message: {s3Ex.Message}. Please check: 1) Ensure the Account ID ({_accountId}) in S3ServiceUrl matches the account of the API Token 2) Confirm the Token's Account ID on the Cloudflare Dashboard R2 API Tokens page 3) If Account ID differs, create a new Token or update S3ServiceUrl configuration",
                _ => $"Upload failed: {s3Ex.ErrorCode} - {s3Ex.Message}"
            };
            
            throw new InvalidOperationException(errorMessage, s3Ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to upload file to R2: {ObjectKey}, Error: {Error}", objectKey, ex.Message);
            throw new InvalidOperationException($"Failed to upload file to R2: {ex.Message}", ex);
        }
    }

    private static string GenerateVideoKey(string fileName)
    {
        var extension = Path.GetExtension(fileName);
        var uniqueId = Guid.NewGuid().ToString("N");
        var timestamp = DateTime.UtcNow.ToString("yyyyMMdd");
        // No need to include bucket name as it's already specified in BucketName parameter
        return $"{timestamp}/highlight-{uniqueId}{extension}";
    }

    private static string GenerateThumbnailKey(string fileName)
    {
        var extension = Path.GetExtension(fileName);
        var uniqueId = Guid.NewGuid().ToString("N");
        var timestamp = DateTime.UtcNow.ToString("yyyyMMdd");
        // No need to include bucket name as it's already specified in BucketName parameter
        return $"{timestamp}/thumb-{uniqueId}{extension}";
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

    private static string GetImageContentType(string fileName)
    {
        var extension = Path.GetExtension(fileName).ToLowerInvariant();
        return extension switch
        {
            ".jpg" => "image/jpeg",
            ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            ".webp" => "image/webp",
            _ => "image/jpeg" // Default value
        };
    }

    public void Dispose()
    {
        _s3Client?.Dispose();
    }
}
