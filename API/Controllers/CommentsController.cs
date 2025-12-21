using Application.Common.Interfaces;
using Application.DTOs.Comment;
using Application.Features.Comments.Commands.CreateComment;
using Application.Features.Comments.Commands.UpdateComment;
using Application.Features.Comments.Commands.DeleteComment;
using Application.Features.Comments.Queries.GetVideoComments;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using API.SignalR;

namespace API.Controllers;

public class CommentsController : BaseApiController
{
    private readonly IMediator _mediator;
    private readonly ILogger<CommentsController> _logger;
    private readonly IHubContext<CommentHub> _hubContext;
    private readonly IApplicationDbContext _context;

    public CommentsController(
        IMediator mediator,
        ILogger<CommentsController> logger,
        IHubContext<CommentHub> hubContext,
        IApplicationDbContext context)
    {
        _mediator = mediator;
        _logger = logger;
        _hubContext = hubContext;
        _context = context;
    }

    /// <summary>
    /// Create comment
    /// </summary>
    [HttpPost("videos/{videoId}/comments")]
    [Authorize]
    public async Task<ActionResult<CommentDto>> CreateComment(Guid videoId, [FromBody] CreateCommentDto dto)
    {
        try
        {
            var command = new CreateCommentCommand(videoId, dto.Content, dto.ParentCommentId);
            var comment = await _mediator.Send(command);

            // Broadcast updated comments list to all clients watching this video
            var comments = await _mediator.Send(new GetVideoCommentsQuery(videoId));
            await _hubContext.Clients.Group(videoId.ToString()).SendAsync("ReceiveComments", comments);

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
    /// Update comment
    /// </summary>
    [HttpPut("{id}")]
    [Authorize]
    public async Task<ActionResult> UpdateComment(Guid id, [FromBody] UpdateCommentDto dto)
    {
        // Get videoId before updating (in case update fails)
        var comment = await _context.Comments
            .FirstOrDefaultAsync(c => c.Id == id && !c.IsDeleted);

        if (comment == null)
            return NotFound();

        var videoId = comment.VideoId;

        var success = await _mediator.Send(new UpdateCommentCommand(id, dto.Content));
        if (!success)
            return NotFound();

        // Broadcast updated comments list to all clients watching this video
        var comments = await _mediator.Send(new GetVideoCommentsQuery(videoId));
        await _hubContext.Clients.Group(videoId.ToString()).SendAsync("ReceiveComments", comments);

        return NoContent();
    }

    /// <summary>
    /// Delete comment
    /// </summary>
    [HttpDelete("{id}")]
    [Authorize]
    public async Task<ActionResult> DeleteComment(Guid id)
    {
        // Get videoId before deleting (in case delete fails)
        var comment = await _context.Comments
            .FirstOrDefaultAsync(c => c.Id == id && !c.IsDeleted);

        if (comment == null)
            return NotFound();

        var videoId = comment.VideoId;

        var success = await _mediator.Send(new DeleteCommentCommand(id));
        if (!success)
            return NotFound();

        // Broadcast updated comments list to all clients watching this video
        var comments = await _mediator.Send(new GetVideoCommentsQuery(videoId));
        await _hubContext.Clients.Group(videoId.ToString()).SendAsync("ReceiveComments", comments);

        return NoContent();
    }
}

