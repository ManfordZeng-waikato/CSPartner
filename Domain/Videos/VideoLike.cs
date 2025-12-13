using System;

using Domain.Common;
using Domain.Users;

namespace Domain.Videos;

public class VideoLike : AuditableEntity
{
    public Guid VideoId { get; private set; }
    public HighlightVideo Video { get; private set; } = default!;

    public Guid UserId { get; private set; }

    private VideoLike() { }

    public VideoLike(Guid videoId, Guid userId)
    {
        VideoId = videoId;
        UserId = userId;
    }
}
