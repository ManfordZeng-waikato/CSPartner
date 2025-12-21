using Application.Common.Interfaces;

namespace Application.Features.Comments.Commands.UpdateComment;

public record UpdateCommentCommand(
    Guid CommentId,
    string Content) : ICommand<bool>;

