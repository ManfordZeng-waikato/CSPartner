using Application.DTOs.Comment;
using Application.Features.Comments.Queries.GetVideoComments;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace API.SignalR;

[AllowAnonymous]
public class CommentHub(IMediator mediator) : Hub
{
    // SignalR Hub for real-time comment updates
    public override async Task OnConnectedAsync()
    {
        try
        {
            var httpContext = Context.GetHttpContext();
            var videoIdParam = httpContext?.Request.Query["videoId"].ToString();

            if (string.IsNullOrWhiteSpace(videoIdParam))
            {
                throw new HubException("videoId is required");
            }

            if (!Guid.TryParse(videoIdParam, out var videoId))
            {
                throw new HubException("Invalid videoId format");
            }

            // Add connection to group for this video
            await Groups.AddToGroupAsync(Context.ConnectionId, videoId.ToString());

            // Send current comments to the newly connected client
            var comments = await mediator.Send(new GetVideoCommentsQuery(videoId));
            await Clients.Caller.SendAsync("LoadComments", comments);
        }
        catch (HubException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new HubException($"Failed to connect: {ex.Message}");
        }
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        // Clean up is handled automatically by SignalR
        await base.OnDisconnectedAsync(exception);
    }
}