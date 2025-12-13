using System;

namespace Domain.Common;
/// <summary>
/// 领域实体基类：统一 Id
/// </summary>
public abstract class BaseEntity
{
    public Guid Id { get; protected set; } = Guid.NewGuid();
}
