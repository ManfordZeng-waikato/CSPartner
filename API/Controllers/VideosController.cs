using API.DTOs;
using Application.DTOs.Video;
using Application.DTOs.Comment;
using Application.Features.Videos.Commands.CreateVideo;
using Application.Features.Videos.Commands.UploadAndCreateVideo;
using Application.Features.Videos.Commands.UpdateVideo;
using Application.Features.Videos.Commands.DeleteVideo;
using Application.Features.Videos.Commands.ToggleLike;
using Application.Features.Videos.Commands.IncreaseViewCount;
using Application.Features.Videos.Queries.GetVideos;
using Application.Features.Videos.Queries.GetVideoById;
using Application.Features.Videos.Queries.GetVideosByUserId;
using Application.Features.Comments.Queries.GetVideoComments;
using Domain.Videos;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace API.Controllers;

public class VideosController : BaseApiController
{
    private readonly IMediator _mediator;
    private readonly ILogger<VideosController> _logger;

    public VideosController(IMediator mediator, ILogger<VideosController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Get video list
    /// Visibility rules:
    /// - Anonymous users: only Public videos
    /// - Authenticated users: Public videos + own Private videos
    /// </summary>
    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<IEnumerable<VideoDto>>> GetVideos([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        if (page < 1) page = 1;
        if (pageSize < 1 || pageSize > 100) pageSize = 20;

        var currentUserId = GetCurrentUserId();
        var videos = await _mediator.Send(new GetVideosQuery(page, pageSize, currentUserId));
        return Ok(videos);
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
        var currentUserId = GetCurrentUserId();
        var videos = await _mediator.Send(new GetVideosByUserIdQuery(userId, currentUserId));
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
        var currentUserId = GetCurrentUserId();
        var video = await _mediator.Send(new GetVideoByIdQuery(id, currentUserId));
        if (video == null)
            return NotFound();

        // Increment view count
        await _mediator.Send(new IncreaseViewCountCommand(id));

        return Ok(video);
    }

    /// <summary>
    /// Upload video file to R2 and create video record
    /// </summary>
    [HttpPost("upload")]
    [Authorize]
    [RequestSizeLimit(50_000_000)] // 50MB limit
    public async Task<ActionResult<VideoDto>> UploadVideo(
        [FromForm] UploadVideoFormRequest formRequest,
        CancellationToken cancellationToken = default)
    {
        var uploaderUserId = GetCurrentUserId();
        if (!uploaderUserId.HasValue)
        {
            _logger.LogWarning("无法从JWT令牌中提取用户ID。Claims: {Claims}", 
                string.Join(", ", User.Claims.Select(c => $"{c.Type}={c.Value}")));
            return Unauthorized(new { error = "无法识别用户身份" });
        }

        // Basic validation
        if (formRequest.VideoFile == null || formRequest.VideoFile.Length == 0)
            return BadRequest(new { error = "视频文件不能为空" });

        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            // Convert API layer form request to application layer request
            var uploadRequest = new UploadVideoRequest
            {
                VideoStream = formRequest.VideoFile.OpenReadStream(),
                VideoFileName = formRequest.VideoFile.FileName,
                Title = formRequest.Title,
                Description = formRequest.Description,
                Visibility = formRequest.Visibility
            };

            // Handle thumbnail (if provided)
            if (formRequest.ThumbnailFile != null && formRequest.ThumbnailFile.Length > 0)
            {
                uploadRequest.ThumbnailStream = formRequest.ThumbnailFile.OpenReadStream();
                uploadRequest.ThumbnailFileName = formRequest.ThumbnailFile.FileName;
            }

            // Call MediatR to handle upload and creation
            var command = new UploadAndCreateVideoCommand(
                uploaderUserId.Value,
                uploadRequest.VideoStream,
                uploadRequest.VideoFileName,
                uploadRequest.Title,
                uploadRequest.Description,
                uploadRequest.ThumbnailStream,
                uploadRequest.ThumbnailFileName,
                uploadRequest.Visibility);

            var video = await _mediator.Send(command, cancellationToken);

            return CreatedAtAction(nameof(GetVideo), new { id = video.VideoId }, video);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Video upload validation failed");
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Video upload failed");
            return BadRequest(new { error = $"上传失败: {ex.Message}" });
        }
    }

    /// <summary>
    /// Create video record (using uploaded R2 URL)
    /// </summary>
    [HttpPost]
    [Authorize]
    public async Task<ActionResult<VideoDto>> CreateVideo([FromBody] CreateVideoDto dto)
    {
        var uploaderUserId = GetCurrentUserId();
        if (!uploaderUserId.HasValue)
        {
            _logger.LogWarning("无法从JWT令牌中提取用户ID。Claims: {Claims}", 
                string.Join(", ", User.Claims.Select(c => $"{c.Type}={c.Value}")));
            return Unauthorized(new { error = "无法识别用户身份" });
        }

        try
        {
            var command = new CreateVideoCommand(
                uploaderUserId.Value,
                dto.Title,
                dto.VideoUrl,
                dto.Description,
                dto.ThumbnailUrl,
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
    public async Task<ActionResult> UpdateVideo(Guid id, [FromBody] UpdateVideoDto dto, [FromQuery] Guid userId)
    {
        var command = new UpdateVideoCommand(
            id,
            userId,
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
    public async Task<ActionResult> DeleteVideo(Guid id, [FromQuery] Guid userId)
    {
        var success = await _mediator.Send(new DeleteVideoCommand(id, userId));
        if (!success)
            return NotFound();

        return NoContent();
    }

    /// <summary>
    /// Like/Unlike video
    /// </summary>
    [HttpPost("{id}/like")]
    public async Task<ActionResult> ToggleLike(Guid id, [FromQuery] Guid userId)
    {
        var success = await _mediator.Send(new ToggleLikeCommand(id, userId));
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
}

