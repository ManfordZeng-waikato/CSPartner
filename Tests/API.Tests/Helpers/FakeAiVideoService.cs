using Application.Common.Interfaces;
using Application.DTOs.Ai;

namespace API.Tests.Helpers;

public class FakeAiVideoService : IAiVideoService
{
    public VideoAiResultDto Result { get; set; } = new("AI description");
    public Exception? ExceptionToThrow { get; set; }

    public Task<VideoAiResultDto> GenerateVideoMetaAsync(VideoAiInputDto input, CancellationToken ct)
    {
        if (ExceptionToThrow != null)
        {
            throw ExceptionToThrow;
        }

        return Task.FromResult(Result);
    }
}
