namespace Application.DTOs.Comment;

public class CommentDto
{
    public Guid CommentId { get; set; }
    public Guid VideoId { get; set; }
    public Guid UserId { get; set; }
    public Guid? ParentCommentId { get; set; }
    public Guid? ParentUserId { get; set; } // UserId of the parent comment (for displaying "Reply to @username")
    public string Content { get; set; } = default!;
    public DateTime CreatedAtUtc { get; set; }
    public DateTime? UpdatedAtUtc { get; set; }
    public List<CommentDto> Replies { get; set; } = [];

}
