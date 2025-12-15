using Application.DTOs;
using Application.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

public class CommentsController : BaseApiController
{
    private readonly IVideoService _videoService;
    private readonly ILogger<CommentsController> _logger;

    public CommentsController(IVideoService videoService, ILogger<CommentsController> logger)
    {
        _videoService = videoService;
        _logger = logger;
    }

    /// <summary>
    /// 创建评论
    /// </summary>
    [HttpPost("videos/{videoId}/comments")]
    public async Task<ActionResult<CommentDto>> CreateComment(Guid videoId, [FromBody] CreateCommentDto dto, [FromQuery] Guid userId)
    {
        try
        {
            var comment = await _videoService.CreateCommentAsync(videoId, userId, dto.Content, dto.ParentCommentId);
            return CreatedAtAction(nameof(CreateComment), new { id = comment.CommentId }, comment);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "创建评论失败");
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "创建评论失败");
            return BadRequest(new { error = "创建评论时发生错误" });
        }
    }

    /// <summary>
    /// 删除评论
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteComment(Guid id, [FromQuery] Guid userId)
    {
        var success = await _videoService.DeleteCommentAsync(id, userId);
        if (!success)
            return NotFound();

        return NoContent();
    }
}

