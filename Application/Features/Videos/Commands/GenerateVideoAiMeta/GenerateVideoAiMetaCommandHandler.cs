using System.Text.Json;
using Application.Common.Interfaces;
using Application.DTOs.Ai;
using Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Application.Features.Videos.Commands.GenerateVideoAiMeta;

public class GenerateVideoAiMetaCommandHandler
    : IRequestHandler<GenerateVideoAiMetaCommand, VideoAiResultDto>
{
    private readonly IApplicationDbContext _context;
    private readonly IAiVideoService _ai;
    private readonly ILogger<GenerateVideoAiMetaCommandHandler> _logger;

    public GenerateVideoAiMetaCommandHandler(
        IApplicationDbContext context,
        IAiVideoService ai,
        ILogger<GenerateVideoAiMetaCommandHandler> logger)
    {
        _context = context;
        _ai = ai;
        _logger = logger;
    }

    public async Task<VideoAiResultDto> Handle(GenerateVideoAiMetaCommand request, CancellationToken cancellationToken)
    {
        var video = await _context.Videos
            .FirstOrDefaultAsync(v => v.Id == request.VideoId && !v.IsDeleted, cancellationToken);

        if (video == null)
            throw new VideoNotFoundException(request.VideoId);

        // Mark as pending before calling AI (for observability)
        video.MarkAiPending();
        await _context.SaveChangesAsync(cancellationToken);

        try
        {
            var input = new VideoAiInputDto(
                Title: video.Title,
                UserDescription: video.Description,
                Map: null,
                Mode: null,
                Weapon: null,
                ExtraContext: "CS2 highlight video"
            );

            _logger.LogInformation("Starting AI metadata generation for video {VideoId}", request.VideoId);
            var result = await _ai.GenerateVideoMetaAsync(input, cancellationToken);

            // Store tags as JSON array string in DB
            var normalizedTags = NormalizeTags(result.Tags);
            var tagsJson = JsonSerializer.Serialize(normalizedTags);

            video.MarkAiCompleted(result.Description, tagsJson, result.HighlightType);

            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Successfully generated AI metadata for video {VideoId}", request.VideoId);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to generate AI metadata for video {VideoId}", request.VideoId);

            var safeError = ToSafeDbError(ex);
            video.MarkAiFailed(safeError);
            await _context.SaveChangesAsync(cancellationToken);
            throw;
        }
    }

    // Keeps error text safe for DB storage and avoids leaking sensitive details.
    private static string ToSafeDbError(Exception ex, int maxLen = 1000)
    {
        var msg = ex.InnerException?.Message ?? ex.Message;
        msg = msg?.Trim() ?? "Unknown error";

        // Hard cap to prevent DB update failures.
        return msg.Length <= maxLen ? msg : msg[..maxLen];
    }

    // Normalizes AI tags for stable storage and better search/recommendation later.
    private static IReadOnlyList<string> NormalizeTags(IEnumerable<string> tags)
    {
        return tags
            .Select(t => (t ?? string.Empty).Trim().ToLowerInvariant())
            .Where(t => t.Length >= 2 && t.Length <= 24)
            .Distinct()
            .Take(8)
            .ToList();
    }
}
