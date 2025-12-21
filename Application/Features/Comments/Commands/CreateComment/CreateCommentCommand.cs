using Application.Common.Interfaces;
using Application.DTOs.Comment;

namespace Application.Features.Comments.Commands.CreateComment;

public record CreateCommentCommand(
    Guid VideoId,
    string Content,
    Guid? ParentCommentId = null) : ICommand<CommentDto>;

