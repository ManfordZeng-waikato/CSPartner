using System;

using Domain.Common;
using Domain.Users;
using Domain.Videos;

namespace Domain.Comments;

public class Comment : AuditableEntity
{
    public Guid CommentId => Id;

    public Guid VideoId { get; private set; }
    public HighlightVideo Video { get; private set; } = default!;

    public Guid UserId { get; private set; }

    // 楼中楼：父评论
    public Guid? ParentCommentId { get; private set; }
    public Comment? ParentComment { get; private set; }
    public ICollection<Comment> Replies { get; private set; } = new List<Comment>();

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
            throw new ArgumentException("评论内容不能为空", nameof(content));

        var s = content.Trim();
        Content = s.Length <= 2000 ? s : s[..2000];
        Touch();
    }

    public void SoftDelete(string? reason = null)
    {
        IsDeleted = true;
        DeletedReason = string.IsNullOrWhiteSpace(reason) ? null : reason.Trim();
        // 也可以选择清空内容，避免敏感信息泄露
        Content = "该评论已删除";
        Touch();
    }
}
