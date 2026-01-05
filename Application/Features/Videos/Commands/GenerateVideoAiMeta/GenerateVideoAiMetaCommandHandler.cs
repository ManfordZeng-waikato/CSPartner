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
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<GenerateVideoAiMetaCommandHandler> _logger;

    public GenerateVideoAiMetaCommandHandler(
        IApplicationDbContext context,
        IAiVideoService ai,
        ICurrentUserService currentUserService,
        ILogger<GenerateVideoAiMetaCommandHandler> logger)
    {
        _context = context;
        _ai = ai;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    public async Task<VideoAiResultDto> Handle(GenerateVideoAiMetaCommand request, CancellationToken cancellationToken)
    {
        if (!_currentUserService.UserId.HasValue)
            throw AuthenticationRequiredException.ForOperation("generate AI metadata for a video");

        var video = await _context.Videos
            .FirstOrDefaultAsync(v => v.Id == request.VideoId && !v.IsDeleted, cancellationToken);

        if (video == null)
            throw new VideoNotFoundException(request.VideoId);

        if (video.UploaderUserId != _currentUserService.UserId.Value)
            throw new UnauthorizedOperationException("video", request.VideoId);

        // Mark as pending before calling AI (for observability)
        video.MarkAiPending();
        await _context.SaveChangesAsync(cancellationToken);

        try
        {
            var input = new VideoAiInputDto(
                Title: video.Title,
                UserDescription: video.Description,
                Map: request.Map,
                Mode: null,
                Weapon: request.Weapon,
                ExtraContext: "CS2 highlight video"
            );

            _logger.LogInformation("Starting AI metadata generation for video {VideoId}", request.VideoId);
            var result = await _ai.GenerateVideoMetaAsync(input, cancellationToken);

            // Service already normalizes tags, so we can serialize directly
            var tagsJson = JsonSerializer.Serialize(result.Tags);

            video.MarkAiCompleted(result.Description, tagsJson, result.HighlightType);

            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Successfully generated AI metadata for video {VideoId}", request.VideoId);
            return result;
        }
        catch (AiServiceQuotaExceededException ex)
        {
            // Handle quota exceeded errors with specific logging
            _logger.LogWarning(
                ex,
                "AI service quota exceeded for video {VideoId}. Video status will be marked as failed. Please check billing or wait for quota reset.",
                request.VideoId);

            var safeError = ToSafeDbError(ex);
            video.MarkAiFailed(safeError);
            await _context.SaveChangesAsync(cancellationToken);
            throw;
        }
        catch (AiServiceException ex)
        {
            // Handle AI service errors with status code logging
            _logger.LogError(
                ex,
                "AI service error for video {VideoId}. StatusCode={StatusCode}, Message={Message}",
                request.VideoId, ex.StatusCode, ex.Message);

            var safeError = ToSafeDbError(ex);
            video.MarkAiFailed(safeError);
            await _context.SaveChangesAsync(cancellationToken);
            throw;
        }
        catch (Exception ex)
        {
            // Handle other unexpected errors
            _logger.LogError(ex, "Unexpected error while generating AI metadata for video {VideoId}", request.VideoId);

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
}
