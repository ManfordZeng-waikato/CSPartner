using Application.DTOs.Comment;
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
    /// Create comment
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
            _logger.LogWarning(ex, "Failed to create comment");
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create comment");
            return BadRequest(new { error = "创建评论时发生错误" });
        }
    }

    /// <summary>
    /// Delete comment
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

