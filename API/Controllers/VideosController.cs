using API.DTOs;
using Application.DTOs;
using Application.Interfaces.Services;
using Domain.Videos;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

public class VideosController : BaseApiController
{
    private readonly IVideoService _videoService;
    private readonly ILogger<VideosController> _logger;

    public VideosController(IVideoService videoService, ILogger<VideosController> logger)
    {
        _videoService = videoService;
        _logger = logger;
    }

    /// <summary>
    /// 获取视频列表
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<VideoDto>>> GetVideos([FromQuery] int page = 1, [FromQuery] int pageSize = 20, [FromQuery] Guid? userId = null)
    {
        if (page < 1) page = 1;
        if (pageSize < 1 || pageSize > 100) pageSize = 20;

        var videos = await _videoService.GetVideosAsync(page, pageSize, userId);
        return Ok(videos);
    }

    /// <summary>
    /// 获取视频详情
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<VideoDto>> GetVideo(Guid id, [FromQuery] Guid? userId = null)
    {
        var video = await _videoService.GetVideoByIdAsync(id, userId);
        if (video == null)
            return NotFound();

        // 增加观看次数
        await _videoService.IncreaseViewCountAsync(id);

        return Ok(video);
    }

    /// <summary>
    /// 上传视频文件到 R2 并创建视频记录
    /// </summary>
    [HttpPost("upload")]
    [RequestSizeLimit(50_000_000)] // 50MB 限制
    public async Task<ActionResult<VideoDto>> UploadVideo(
        [FromForm] UploadVideoFormRequest formRequest,
        [FromForm] Guid uploaderUserId,
        CancellationToken cancellationToken = default)
    {
        // 基本验证
        if (formRequest.VideoFile == null || formRequest.VideoFile.Length == 0)
            return BadRequest(new { error = "视频文件不能为空" });

        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            // 将 API 层的表单请求转换为应用层的请求
            var uploadRequest = new UploadVideoRequest
            {
                VideoStream = formRequest.VideoFile.OpenReadStream(),
                VideoFileName = formRequest.VideoFile.FileName,
                Title = formRequest.Title,
                Description = formRequest.Description,
                Visibility = formRequest.Visibility
            };

            // 处理缩略图（如果提供）
            if (formRequest.ThumbnailFile != null && formRequest.ThumbnailFile.Length > 0)
            {
                uploadRequest.ThumbnailStream = formRequest.ThumbnailFile.OpenReadStream();
                uploadRequest.ThumbnailFileName = formRequest.ThumbnailFile.FileName;
            }

            // 调用应用服务处理上传和创建
            var video = await _videoService.UploadAndCreateVideoAsync(uploaderUserId, uploadRequest, cancellationToken);

            return CreatedAtAction(nameof(GetVideo), new { id = video.VideoId }, video);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "上传视频验证失败");
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "上传视频失败");
            return BadRequest(new { error = $"上传失败: {ex.Message}" });
        }
    }

    /// <summary>
    /// 创建视频记录（使用已上传的 R2 URL）
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<VideoDto>> CreateVideo([FromBody] CreateVideoDto dto, [FromQuery] Guid uploaderUserId)
    {
        try
        {
            var video = await _videoService.CreateVideoAsync(
                uploaderUserId,
                dto.Title,
                dto.VideoUrl,
                dto.Description,
                dto.ThumbnailUrl,
                dto.Visibility);

            return CreatedAtAction(nameof(GetVideo), new { id = video.VideoId }, video);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "创建视频失败");
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// 更新视频
    /// </summary>
    [HttpPut("{id}")]
    public async Task<ActionResult> UpdateVideo(Guid id, [FromBody] UpdateVideoDto dto, [FromQuery] Guid userId)
    {
        var success = await _videoService.UpdateVideoAsync(
            id,
            userId,
            dto.Title,
            dto.Description,
            dto.ThumbnailUrl,
            dto.Visibility);

        if (!success)
            return NotFound();

        return NoContent();
    }

    /// <summary>
    /// 删除视频
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteVideo(Guid id, [FromQuery] Guid userId)
    {
        var success = await _videoService.DeleteVideoAsync(id, userId);
        if (!success)
            return NotFound();

        return NoContent();
    }

    /// <summary>
    /// 点赞/取消点赞视频
    /// </summary>
    [HttpPost("{id}/like")]
    public async Task<ActionResult> ToggleLike(Guid id, [FromQuery] Guid userId)
    {
        var success = await _videoService.ToggleLikeAsync(id, userId);
        if (!success)
            return NotFound();

        return NoContent();
    }

    /// <summary>
    /// 获取视频评论列表
    /// </summary>
    [HttpGet("{id}/comments")]
    public async Task<ActionResult<IEnumerable<CommentDto>>> GetVideoComments(Guid id)
    {
        var comments = await _videoService.GetVideoCommentsAsync(id);
        return Ok(comments);
    }
}

