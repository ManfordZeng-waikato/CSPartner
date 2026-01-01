using Application.Common.Interfaces;
using Application.DTOs.Comment;
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
    /// Delete comment
    /// </summary>
    [HttpDelete("{id}")]
    [Authorize]
    public async Task<ActionResult> DeleteComment(Guid id)
    {
        // Get videoId before deleting (for SignalR broadcast after successful delete)
        var comment = await _context.Comments
            .FirstOrDefaultAsync(c => c.Id == id && !c.IsDeleted);

        var videoId = comment?.VideoId ?? Guid.Empty;

        await _mediator.Send(new DeleteCommentCommand(id));

        // Broadcast updated comments list to all clients watching this video
        if (videoId != Guid.Empty)
        {
            var comments = await _mediator.Send(new GetVideoCommentsQuery(videoId));
            await _hubContext.Clients.Group(videoId.ToString()).SendAsync("ReceiveComments", comments);
        }

        return NoContent();
    }
}

