using Application.Common.Interfaces;

namespace Application.Features.Comments.Commands.DeleteComment;

public record DeleteCommentCommand(Guid CommentId) : ICommand<bool>;

