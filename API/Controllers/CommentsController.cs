using Application.DTOs.Comment;
using Application.Features.Comments.Commands.CreateComment;
using Application.Features.Comments.Commands.DeleteComment;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

public class CommentsController : BaseApiController
{
    private readonly IMediator _mediator;
    private readonly ILogger<CommentsController> _logger;

    public CommentsController(IMediator mediator, ILogger<CommentsController> logger)
    {
        _mediator = mediator;
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
            var command = new CreateCommentCommand(videoId, userId, dto.Content, dto.ParentCommentId);
            var comment = await _mediator.Send(command);
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
        var success = await _mediator.Send(new DeleteCommentCommand(id, userId));
        if (!success)
            return NotFound();

        return NoContent();
    }
}

