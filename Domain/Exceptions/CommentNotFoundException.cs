namespace Domain.Exceptions;

public class CommentNotFoundException : DomainException
{
    public CommentNotFoundException(Guid commentId) 
        : base($"评论 {commentId} 不存在或已删除")
    {
    }
}
