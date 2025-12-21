using Application.Common.Interfaces;

namespace Application.Features.Comments.Commands.UpdateComment;

public record UpdateCommentCommand(
    Guid CommentId,
    Guid UserId,
    string Content) : ICommand<bool>;

