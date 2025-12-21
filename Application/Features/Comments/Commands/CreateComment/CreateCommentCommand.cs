using Application.Common.Interfaces;
using Application.DTOs.Comment;

namespace Application.Features.Comments.Commands.CreateComment;

public record CreateCommentCommand(
    Guid VideoId,
    Guid UserId,
    string Content,
    Guid? ParentCommentId = null) : ICommand<CommentDto>;

