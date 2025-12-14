using Application.DTOs;
using Application.Interfaces.Services;
using Application.Services;
using Domain.Comments;
using Domain.Videos;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

public class VideosController : BaseApiController
{
    private readonly IVideoService _videoService;
    private readonly IStorageService _storageService;
    private readonly ILogger<VideosController> _logger;

    public VideosController(IVideoService videoService, IStorageService storageService, ILogger<VideosController> logger)
    {
        _videoService = videoService;
        _storageService = storageService;
        _logger = logger;
    }

    /// <summary>
    /// 获取视频列表
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<VideoDto>>> GetVideos([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        if (page < 1) page = 1;
        if (pageSize < 1 || pageSize > 100) pageSize = 20;

        var videos = await _videoService.GetVideosAsync(page, pageSize);
        var videoDtos = videos.Select(v => MapToDto(v, null)).ToList();

        return Ok(videoDtos);
    }

    /// <summary>
    /// 获取视频详情
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<VideoDto>> GetVideo(Guid id, [FromQuery] Guid? userId = null)
    {
        var video = await _videoService.GetVideoByIdAsync(id);
        if (video == null)
            return NotFound();

        // 增加观看次数
        await _videoService.IncreaseViewCountAsync(id);

        // 检查用户是否已点赞
        bool hasLiked = false;
        if (userId.HasValue)
        {
            hasLiked = await _videoService.HasUserLikedAsync(id, userId.Value);
        }

        var videoDto = MapToDto(video, userId);
        videoDto.HasLiked = hasLiked;

        return Ok(videoDto);
    }

    /// <summary>
    /// 上传视频文件到 R2
    /// </summary>
    [HttpPost("upload")]
    [RequestSizeLimit(50_000_000)] // 50MB 限制
    public async Task<ActionResult<UploadVideoResponseDto>> UploadVideo(
        [FromForm] IFormFile videoFile,
        [FromForm] IFormFile? thumbnailFile = null,
        CancellationToken cancellationToken = default)
    {
        if (videoFile == null || videoFile.Length == 0)
            return BadRequest(new { error = "视频文件不能为空" });

        // 验证文件类型
        var allowedVideoExtensions = new[] { ".mp4", ".webm", ".mov", ".avi" };
        var fileExtension = Path.GetExtension(videoFile.FileName).ToLowerInvariant();
        if (!allowedVideoExtensions.Contains(fileExtension))
            return BadRequest(new { error = "不支持的视频格式，支持: mp4, webm, mov, avi" });

        try
        {
            // 上传视频到 R2
            string videoUrl;
            using (var videoStream = videoFile.OpenReadStream())
            {
                videoUrl = await _storageService.UploadVideoAsync(videoStream, videoFile.FileName, cancellationToken);
            }

            // 上传缩略图（如果提供）
            string? thumbnailUrl = null;
            if (thumbnailFile != null && thumbnailFile.Length > 0)
            {
                var allowedImageExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp" };
                var imageExtension = Path.GetExtension(thumbnailFile.FileName).ToLowerInvariant();
                if (allowedImageExtensions.Contains(imageExtension))
                {
                    using (var thumbnailStream = thumbnailFile.OpenReadStream())
                    {
                        thumbnailUrl = await _storageService.UploadThumbnailAsync(thumbnailStream, thumbnailFile.FileName, cancellationToken);
                    }
                }
            }

            return Ok(new UploadVideoResponseDto
            {
                VideoUrl = videoUrl,
                ThumbnailUrl = thumbnailUrl,
                Message = "视频上传成功"
            });
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

            var videoDto = MapToDto(video, uploaderUserId);
            return CreatedAtAction(nameof(GetVideo), new { id = video.VideoId }, videoDto);
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
        var commentDtos = comments.Select(c => MapCommentToDto(c)).ToList();
        return Ok(commentDtos);
    }

    private static VideoDto MapToDto(HighlightVideo video, Guid? userId)
    {
        return new VideoDto
        {
            VideoId = video.VideoId,
            UploaderUserId = video.UploaderUserId,
            Title = video.Title,
            Description = video.Description,
            VideoUrl = video.VideoUrl,
            ThumbnailUrl = video.ThumbnailUrl,
            LikeCount = video.LikeCount,
            CommentCount = video.CommentCount,
            ViewCount = video.ViewCount,
            Visibility = video.Visibility,
            CreatedAtUtc = video.CreatedAtUtc,
            UpdatedAtUtc = video.UpdatedAtUtc,
            HasLiked = false // 将在调用方设置
        };
    }

    private static CommentDto MapCommentToDto(Comment comment)
    {
        return new CommentDto
        {
            CommentId = comment.CommentId,
            VideoId = comment.VideoId,
            UserId = comment.UserId,
            ParentCommentId = comment.ParentCommentId,
            Content = comment.Content,
            CreatedAtUtc = comment.CreatedAtUtc,
            UpdatedAtUtc = comment.UpdatedAtUtc,
            Replies = comment.Replies.Select(r => MapCommentToDto(r)).ToList()
        };
    }
}

