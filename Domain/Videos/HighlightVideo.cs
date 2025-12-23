using System;
using System.ComponentModel.DataAnnotations;
using Domain.Comments;
using Domain.Common;
using Domain.Users;

namespace Domain.Videos;

public class HighlightVideo : AuditableEntity
{
    public Guid VideoId => Id;

    public Guid UploaderUserId { get; private set; }

    public string Title { get; private set; } = default!;
    public string? Description { get; private set; }

    // DB only stores URL: object storage address
    public string VideoUrl { get; private set; } = default!;
    public string? ThumbnailUrl { get; private set; }

    // Redundant counts (can keep for MVP)
    public int LikeCount { get; private set; }
    public int CommentCount { get; private set; }
    public long ViewCount { get; private set; }

    public VideoVisibility Visibility { get; private set; } = VideoVisibility.Public;

    public bool IsDeleted { get; private set; }

    public ICollection<Comment> Comments { get; private set; } = [];
    public ICollection<VideoLike> Likes { get; private set; } = [];

    private HighlightVideo() { }

    public HighlightVideo(Guid uploaderUserId, string title, string videoUrl, string? description = null, string? thumbnailUrl = null)
    {
        UploaderUserId = uploaderUserId;
        SetTitle(title);
        SetVideoUrl(videoUrl);
        SetDescription(description);
        SetThumbnailUrl(thumbnailUrl);
    }

    public void SetTitle(string title)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Title cannot be empty", nameof(title));

        Title = title.Trim().Length <= 120 ? title.Trim() : title.Trim()[..120];
        Touch();
    }

    public void SetDescription(string? description)
    {
        if (string.IsNullOrWhiteSpace(description))
        {
            Description = null;
        }
        else
        {
            var s = description.Trim();
            Description = s.Length <= 2000 ? s : s[..2000];
        }
        Touch();
    }

    public void SetVideoUrl(string url)
    {
        if (string.IsNullOrWhiteSpace(url))
            throw new ArgumentException("VideoUrl cannot be empty", nameof(url));

        VideoUrl = url.Trim();
        Touch();
    }

    public void SetThumbnailUrl(string? url)
    {
        ThumbnailUrl = NormalizeUrl(url);
        Touch();
    }

    public void IncreaseView() => ViewCount++;

    public void ApplyLikeAdded()
    {
        LikeCount++;
        Touch();
    }

    public void ApplyLikeRemoved()
    {
        if (LikeCount > 0) LikeCount--;
        Touch();
    }

    public void ApplyCommentAdded()
    {
        CommentCount++;
        Touch();
    }

    public void ApplyCommentRemoved()
    {
        if (CommentCount > 0) CommentCount--;
        Touch();
    }

    public void SetVisibility(VideoVisibility visibility)
    {
        Visibility = visibility;
        Touch();
    }

    public void SoftDelete()
    {
        IsDeleted = true;
        Touch();
    }

    private static string? NormalizeUrl(string? url)
        => string.IsNullOrWhiteSpace(url) ? null : url.Trim();
}
