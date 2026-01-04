using Application.DTOs.Ai;

namespace Application.Common.Interfaces;

public interface IAiVideoService
{
    // Generates AI metadata for a video based on provided context.
    Task<VideoAiResultDto> GenerateVideoMetaAsync(VideoAiInputDto input, CancellationToken ct);
}
