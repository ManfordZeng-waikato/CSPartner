namespace Application.DTOs.Storage;

public record PreSignedUploadResult(
    string UploadUrl,
    string ObjectKey,
    string PublicUrl,
    DateTime ExpiresAtUtc,
    string ContentType);

