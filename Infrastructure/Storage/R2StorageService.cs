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
                "AccessDenied" => "访问被拒绝。请检查：1) AccessKeyId 和 SecretAccessKey 是否正确 2) API Token 是否有写入权限 3) Bucket 名称是否正确",
                "NoSuchBucket" => $"Bucket '{_bucketName}' 不存在。请检查 Bucket 名称是否正确",
                "InvalidAccessKeyId" => "AccessKeyId 无效。请检查配置是否正确",
                "SignatureDoesNotMatch" => "签名不匹配。请检查 SecretAccessKey 是否正确",
                "PermanentRedirect" => $"Endpoint 配置错误。当前使用的 S3ServiceUrl: {_s3ServiceUrl}。错误消息: {s3Ex.Message}。请检查：1) 确保 S3ServiceUrl 中的 Account ID ({_accountId}) 与 API Token 所属账户匹配 2) 在 Cloudflare Dashboard 的 R2 API Tokens 页面确认 Token 所属的 Account ID 3) 如果 Account ID 不同，请创建新 Token 或更新 S3ServiceUrl 配置",
                _ => $"上传失败: {s3Ex.ErrorCode} - {s3Ex.Message}"
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
