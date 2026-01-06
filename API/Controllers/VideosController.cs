using API.DTOs;
using Application.Common.Interfaces;
using Application.DTOs.Common;
using Application.DTOs.Video;
using Application.DTOs.Comment;
using Application.Features.Videos.Commands.CreateVideo;
using Application.Features.Videos.Commands.UpdateVideo;
using Application.Features.Videos.Commands.DeleteVideo;
using Application.Features.Videos.Commands.ToggleLike;
using Application.Features.Videos.Commands.IncreaseViewCount;
using Application.Features.Videos.Queries.GetVideos;
using Application.Features.Videos.Queries.GetVideoById;
using Application.Features.Videos.Queries.GetVideosByUserId;
using Application.Features.Videos.Queries.GetVideoUploadUrl;
using Application.Features.Comments.Commands.CreateComment;
using Application.Features.Comments.Queries.GetVideoComments;
using Domain.Videos;
using Domain.Exceptions;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using API.SignalR;
using Application.DTOs.Ai;
using Application.Features.Videos.Commands.GenerateVideoAiMeta;
using Microsoft.Extensions.DependencyInjection;

namespace API.Controllers;

public class VideosController : BaseApiController
{
    private readonly IMediator _mediator;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<VideosController> _logger;
    private readonly IHubContext<CommentHub> _hubContext;
    private readonly IServiceScopeFactory _serviceScopeFactory;

    public VideosController(
        IMediator mediator,
        ICurrentUserService currentUserService,
        ILogger<VideosController> logger,
        IHubContext<CommentHub> hubContext,
        IServiceScopeFactory serviceScopeFactory)
    {
        _mediator = mediator;
        _currentUserService = currentUserService;
        _logger = logger;
        _hubContext = hubContext;
        _serviceScopeFactory = serviceScopeFactory;
    }

    /// <summary>
    /// Get video list with cursor pagination
    /// Visibility rules:
    /// - Anonymous users: only Public videos
    /// - Authenticated users: Public videos + own Private videos
    /// Sorting: CreatedAt DESC + Id DESC
    /// </summary>
    [HttpGet]
    [AllowAnonymous]
    [ResponseCache(Duration = 60, Location = ResponseCacheLocation.Any, VaryByQueryKeys = new[] { "cursor", "pageSize" })]
    public async Task<ActionResult<CursorPagedResult<VideoDto>>> GetVideos([FromQuery] string? cursor = null, [FromQuery] int pageSize = 20)
    {
        if (pageSize < 1 || pageSize > 100) pageSize = 20;

        var result = await _mediator.Send(new GetVideosQuery(cursor, pageSize, _currentUserService.UserId));
        return Ok(result);
    }

    /// <summary>
    /// Get all videos uploaded by a specific user
    /// Visibility rules:
    /// - Anonymous users: only Public videos
    /// - Viewing own videos: all videos (Public + Private)
    /// - Viewing others' videos: only Public videos
    /// </summary>
    [HttpGet("user/{userId}")]
    [AllowAnonymous]
    public async Task<ActionResult<IEnumerable<VideoDto>>> GetVideosByUser(Guid userId)
    {
        var videos = await _mediator.Send(new GetVideosByUserIdQuery(userId, _currentUserService.UserId));
        return Ok(videos);
    }

    /// <summary>
    /// Get video details
    /// Visibility rules:
    /// - Anonymous users: only Public videos
    /// - Authenticated users: Public videos + own Private videos
    /// </summary>
    [HttpGet("{id}")]
    [AllowAnonymous]
    public async Task<ActionResult<VideoDto>> GetVideo(Guid id)
    {
        var video = await _mediator.Send(new GetVideoByIdQuery(id, _currentUserService.UserId));
        if (video == null)
            return NotFound();

        // Increment view count
        await _mediator.Send(new IncreaseViewCountCommand(id));

        return Ok(video);
    }

    /// <summary>
    /// Generate a pre-signed URL for uploading a video directly to R2.
    /// 
    /// IMPORTANT: When uploading to the returned UploadUrl, you MUST include the Content-Type header
    /// with the exact value from the response.ContentType field. The pre-signed URL signature includes
    /// this Content-Type, so using a different value will cause the upload to fail.
    /// 
    /// Example upload request:
    /// PUT {UploadUrl}
    /// Content-Type: {response.ContentType}
    /// Body: [video file binary data]
    /// </summary>
    [HttpPost("upload-url")]
    [Authorize]
    public async Task<ActionResult<VideoUploadUrlResponse>> GetUploadUrl(
        [FromBody] VideoUploadUrlRequest request,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.FileName))
            return BadRequest(new { error = "FileName is required" });

        var query = new GetVideoUploadUrlQuery(request.FileName, request.ContentType);
        var result = await _mediator.Send(query, cancellationToken);

        return Ok(new VideoUploadUrlResponse
        {
            UploadUrl = result.UploadUrl,
            ObjectKey = result.ObjectKey,
            PublicUrl = result.PublicUrl,
            ExpiresAtUtc = result.ExpiresAtUtc,
            ContentType = result.ContentType
        });
    }

    /// <summary>
    /// Create video record (using uploaded R2 URL)
    /// </summary>
    [HttpPost]
    [Authorize]
    public async Task<ActionResult<VideoDto>> CreateVideo([FromBody] CreateVideoDto dto, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(dto.VideoObjectKey))
            return BadRequest(new { error = "VideoObjectKey is required" });

        var command = new CreateVideoCommand(
            dto.VideoObjectKey,
            dto.ThumbnailObjectKey,
            dto.Title,
            dto.Description,
            dto.Visibility,
            dto.Map,
            dto.Weapon,
            dto.HighlightType);

        var video = await _mediator.Send(command);

        // Automatically trigger AI metadata generation after video creation
        // Execute in background using IServiceScopeFactory to ensure proper DbContext lifetime
        _ = Task.Run(async () =>
        {
            try
            {
                // Wait a bit longer to ensure video is committed to database
                await Task.Delay(500);
                
                using var scope = _serviceScopeFactory.CreateScope();
                var scopedMediator = scope.ServiceProvider.GetRequiredService<IMediator>();
                var scopedLogger = scope.ServiceProvider.GetRequiredService<ILogger<VideosController>>();
                
                scopedLogger.LogInformation("Starting background AI metadata generation for video {VideoId} with Map={Map}, Weapon={Weapon}, HighlightType={HighlightType}", 
                    video.VideoId, dto.Map, dto.Weapon, dto.HighlightType);
                
                await scopedMediator.Send(new GenerateVideoAiMetaCommand(video.VideoId, dto.Map, dto.Weapon, dto.HighlightType), CancellationToken.None);
                
                scopedLogger.LogInformation("Successfully completed AI metadata generation for video {VideoId}", video.VideoId);
            }
            catch (Exception ex)
            {
                // Use a scoped logger if available, otherwise fall back to instance logger
                try
                {
                    using var errorScope = _serviceScopeFactory.CreateScope();
                    var errorLogger = errorScope.ServiceProvider.GetRequiredService<ILogger<VideosController>>();
                    errorLogger.LogError(ex, "Failed to automatically generate AI metadata for video {VideoId}. User can trigger it manually later.", video.VideoId);
                }
                catch
                {
                    _logger.LogError(ex, "Failed to automatically generate AI metadata for video {VideoId}. User can trigger it manually later.", video.VideoId);
                }
            }
        }, CancellationToken.None);

        return CreatedAtAction(nameof(GetVideo), new { id = video.VideoId }, video);
    }

    /// <summary>
    /// Update video visibility
    /// </summary>
    [HttpPut("{id}")]
    [Authorize]
    public async Task<ActionResult> UpdateVideo(Guid id, [FromBody] UpdateVideoDto dto)
    {
        try
        {
            var command = new UpdateVideoCommand(
                id,
                dto.Visibility);

            await _mediator.Send(command);

            _logger.LogInformation("Successfully updated video {VideoId} visibility to {Visibility}", id, dto.Visibility);
            return NoContent();
        }
        catch (VideoNotFoundException)
        {
            _logger.LogWarning("Video not found: {VideoId}", id);
            return NotFound();
        }
        catch (UnauthorizedOperationException ex)
        {
            _logger.LogWarning("Unauthorized visibility update attempt for video {VideoId} by user {UserId}", id, _currentUserService.UserId);
            return StatusCode(403, new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update video {VideoId} visibility", id);
            return StatusCode(500, new { error = "Failed to update video visibility" });
        }
    }

    /// <summary>
    /// Delete video
    /// </summary>
    [HttpDelete("{id}")]
    [Authorize]
    public async Task<ActionResult> DeleteVideo(Guid id)
    {
        await _mediator.Send(new DeleteVideoCommand(id));

        return NoContent();
    }

    /// <summary>
    /// Like/Unlike video
    /// </summary>
    [HttpPost("{id}/like")]
    [Authorize]
    public async Task<ActionResult> ToggleLike(Guid id)
    {
        await _mediator.Send(new ToggleLikeCommand(id));

        return NoContent();
    }

    /// <summary>
    /// Get video comments list
    /// </summary>
    [HttpGet("{id}/comments")]
    [AllowAnonymous]
    public async Task<ActionResult<IEnumerable<CommentDto>>> GetVideoComments(Guid id)
    {
        var comments = await _mediator.Send(new GetVideoCommentsQuery(id));
        return Ok(comments);
    }

    /// <summary>
    /// Create comment
    /// </summary>
    [HttpPost("{id}/comments")]
    [Authorize]
    public async Task<ActionResult<CommentDto>> CreateComment(Guid id, [FromBody] CreateCommentDto dto)
    {
        try
        {
            var command = new CreateCommentCommand(id, dto.Content, dto.ParentCommentId);
            var comment = await _mediator.Send(command);

            // Broadcast updated comments list to all clients watching this video
            var comments = await _mediator.Send(new GetVideoCommentsQuery(id));
            try
            {
                await _hubContext.Clients.Group(id.ToString()).SendAsync("ReceiveComments", comments);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send SignalR message to group: {GroupName}", id.ToString());
                // Don't fail the request if SignalR fails
            }

            return CreatedAtAction(nameof(GetVideoComments), new { id }, comment);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Failed to create comment");
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create comment");
            return BadRequest(new { error = "Failed to create comment" });
        }
    }

    /// <summary>
    /// Generate AI metadata for a video (description, tags, highlight type)
    /// Only the video uploader can generate AI metadata for their own videos.
    /// </summary>
    [HttpPost("{id}/ai-meta")]
    [Authorize]
    public async Task<ActionResult<VideoAiResultDto>> GenerateAiMeta(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _mediator.Send(new GenerateVideoAiMetaCommand(id), cancellationToken);
            _logger.LogInformation("Successfully generated AI metadata for video {VideoId} by user {UserId}", id, _currentUserService.UserId);
            return Ok(result);
        }
        catch (VideoNotFoundException)
        {
            _logger.LogWarning("Video not found: {VideoId}", id);
            return NotFound();
        }
        catch (UnauthorizedOperationException ex)
        {
            _logger.LogWarning("Unauthorized AI meta generation attempt for video {VideoId} by user {UserId}", id, _currentUserService.UserId);
            return StatusCode(403, new { error = ex.Message });
        }
        catch (AiServiceQuotaExceededException ex)
        {
            _logger.LogError(ex, "AI service quota exceeded for video {VideoId}", id);
            return StatusCode(503, new { error = "AI service quota exceeded. Please try again later." });
        }
        catch (AiServiceException ex)
        {
            _logger.LogError(ex, "AI service error for video {VideoId}. StatusCode={StatusCode}", id, ex.StatusCode);
            return StatusCode(502, new { error = "AI service error. Please try again later." });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error generating AI metadata for video {VideoId}", id);
            return StatusCode(500, new { error = "An error occurred while generating AI metadata." });
        }
    }
}

