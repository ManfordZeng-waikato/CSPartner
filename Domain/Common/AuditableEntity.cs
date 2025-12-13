using System;

namespace Domain.Common;

/// <summary>
/// 审计字段：创建/更新时间
/// </summary>
public abstract class AuditableEntity : BaseEntity
{
    public DateTime CreatedAtUtc { get; protected set; } = DateTime.UtcNow;
    public DateTime? UpdatedAtUtc { get; protected set; }

    public void Touch() => UpdatedAtUtc = DateTime.UtcNow;
}