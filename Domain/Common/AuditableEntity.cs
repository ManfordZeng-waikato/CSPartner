using System;

namespace Domain.Common;

/// <summary>
/// Audit fields: creation/update time
/// </summary>
public abstract class AuditableEntity : BaseEntity
{
    public DateTime CreatedAtUtc { get; protected set; } = DateTime.UtcNow;
    public DateTime? UpdatedAtUtc { get; protected set; }

    public void Touch() => UpdatedAtUtc = DateTime.UtcNow;
}