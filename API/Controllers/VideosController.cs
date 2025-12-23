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
using Application.Features.Comments.Commands.CreateComment;
using Application.Features.Comments.Queries.GetVideoComments;
using Domain.Videos;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using API.SignalR;
using Application.Interfaces.Services;

namespace API.Controllers;

public class VideosController : BaseApiController
{
    private readonly IMediator _mediator;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<VideosController> _logger;
    private readonly IHubContext<CommentHub> _hubContext;
    private readonly IStorageService _storageService;

    public VideosController(
        IMediator mediator,
        ICurrentUserService currentUserService,
        ILogger<VideosController> logger,
        IHubContext<CommentHub> hubContext,
        IStorageService storageService)
    {
        _mediator = mediator;
        _currentUserService = currentUserService;
        _logger = logger;
        _hubContext = hubContext;
        _storageService = storageService;
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
    /// Generate a pre-signed URL for uploading a video directly to R2
    /// </summary>
    [HttpPost("upload-url")]
    [Authorize]
    public async Task<ActionResult<VideoUploadUrlResponse>> GetUploadUrl(
        [FromBody] VideoUploadUrlRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.FileName))
                return BadRequest(new { error = "FileName is required" });

            var contentType = string.IsNullOrWhiteSpace(request.ContentType)
                ? "video/mp4"
                : request.ContentType;

            var result = await _storageService.GetVideoUploadUrlAsync(
                request.FileName,
                contentType,
                cancellationToken);

            var response = new VideoUploadUrlResponse
            {
                UploadUrl = result.UploadUrl,
                ObjectKey = result.ObjectKey,
                PublicUrl = result.PublicUrl,
                ExpiresAtUtc = result.ExpiresAtUtc,
                ContentType = result.ContentType
            };

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to generate pre-signed upload URL");
            return BadRequest(new { error = $"Failed to generate upload URL: {ex.Message}" });
        }
    }

    /// <summary>
    /// Create video record (using uploaded R2 URL)
    /// </summary>
    [HttpPost]
    [Authorize]
    public async Task<ActionResult<VideoDto>> CreateVideo([FromBody] CreateVideoDto dto)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(dto.VideoObjectKey))
            {
                return BadRequest(new { error = "VideoObjectKey is required" });
            }

            // Verify that the video file exists in R2 before creating database record
            // This prevents orphaned database records if R2 upload failed
            var videoExists = await _storageService.FileExistsAsync(dto.VideoObjectKey);
            if (!videoExists)
            {
                _logger.LogWarning("Attempted to create video record for non-existent file: {ObjectKey}", dto.VideoObjectKey);
                return BadRequest(new { error = "Video file not found in storage. Please upload the video again." });
            }

            // Verify thumbnail if provided
            if (!string.IsNullOrWhiteSpace(dto.ThumbnailObjectKey))
            {
                var thumbnailExists = await _storageService.FileExistsAsync(dto.ThumbnailObjectKey);
                if (!thumbnailExists)
                {
                    _logger.LogWarning("Thumbnail file not found: {ObjectKey}", dto.ThumbnailObjectKey);
                    // Don't fail the request if thumbnail is missing, just log it
                }
            }

            var videoUrl = _storageService.GetPublicUrl(dto.VideoObjectKey);
            string? thumbnailUrl = null;
            if (!string.IsNullOrWhiteSpace(dto.ThumbnailObjectKey))
            {
                thumbnailUrl = _storageService.GetPublicUrl(dto.ThumbnailObjectKey);
            }

            var command = new CreateVideoCommand(
                dto.Title,
                videoUrl,
                dto.Description,
                thumbnailUrl,
                dto.Visibility);

            var video = await _mediator.Send(command);

            return CreatedAtAction(nameof(GetVideo), new { id = video.VideoId }, video);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create video");
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Update video
    /// </summary>
    [HttpPut("{id}")]
    [Authorize]
    public async Task<ActionResult> UpdateVideo(Guid id, [FromBody] UpdateVideoDto dto)
    {
        var command = new UpdateVideoCommand(
            id,
            dto.Title,
            dto.Description,
            dto.ThumbnailUrl,
            dto.Visibility);

        var success = await _mediator.Send(command);

        if (!success)
            return NotFound();

        return NoContent();
    }

    /// <summary>
    /// Delete video
    /// </summary>
    [HttpDelete("{id}")]
    [Authorize]
    public async Task<ActionResult> DeleteVideo(Guid id)
    {
        var success = await _mediator.Send(new DeleteVideoCommand(id));
        if (!success)
            return NotFound();

        return NoContent();
    }

    /// <summary>
    /// Like/Unlike video
    /// </summary>
    [HttpPost("{id}/like")]
    [Authorize]
    public async Task<ActionResult> ToggleLike(Guid id)
    {
        var success = await _mediator.Send(new ToggleLikeCommand(id));
        if (!success)
            return NotFound();

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
}

