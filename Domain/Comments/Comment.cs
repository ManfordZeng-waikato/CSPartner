using System;
using System.ComponentModel.DataAnnotations;
using Domain.Common;
using Domain.Users;
using Domain.Videos;

namespace Domain.Comments;

public class Comment : AuditableEntity
{
    public Guid CommentId => Id;

    public Guid VideoId { get; private set; }
    
    [Required]
    public HighlightVideo Video { get; private set; } = default!;   

    public Guid UserId { get; private set; }

    // Threaded comment: parent comment
    public Guid? ParentCommentId { get; private set; }
    public Comment? ParentComment { get; private set; }
    public ICollection<Comment> Replies { get; private set; } = [];

    public string Content { get; private set; } = default!;

    public bool IsDeleted { get; private set; }
    public string? DeletedReason { get; private set; }

    private Comment() { }

    public Comment(Guid videoId, Guid userId, string content, Guid? parentCommentId = null)
    {
        VideoId = videoId;
        UserId = userId;
        ParentCommentId = parentCommentId;
        SetContent(content);
    }

    public void SetContent(string content)
    {
        if (string.IsNullOrWhiteSpace(content))
            throw new ArgumentException("Comment content cannot be empty", nameof(content));

        var s = content.Trim();
        Content = s.Length <= 2000 ? s : s[..2000];
        Touch();
    }

    public void SoftDelete(string? reason = null)
    {
        IsDeleted = true;
        DeletedReason = string.IsNullOrWhiteSpace(reason) ? null : reason.Trim();
        // Optionally clear content to avoid leaking sensitive information
        Content = "This comment has been deleted";
        Touch();
    }
}
