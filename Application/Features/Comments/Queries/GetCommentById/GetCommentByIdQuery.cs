using Application.Common.Interfaces;
using Application.DTOs.Comment;

namespace Application.Features.Comments.Queries.GetCommentById;

public record GetCommentByIdQuery(Guid CommentId) : IQuery<CommentDto>;

