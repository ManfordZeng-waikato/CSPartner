using Application.Common.Interfaces;
using Application.DTOs.Comment;

namespace Application.Features.Comments.Queries.GetVideoComments;

public record GetVideoCommentsQuery(Guid VideoId) : IQuery<IEnumerable<CommentDto>>;

