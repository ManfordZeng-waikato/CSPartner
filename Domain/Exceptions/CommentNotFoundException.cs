namespace Domain.Exceptions;

public class CommentNotFoundException : DomainException
{
    public CommentNotFoundException(Guid commentId) 
        : base($"Comment {commentId} does not exist or has been deleted")
    {
    }
}
