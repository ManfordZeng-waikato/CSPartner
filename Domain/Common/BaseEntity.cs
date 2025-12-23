using System;

namespace Domain.Common;
/// <summary>
/// Domain entity base class: unified Id
/// </summary>
public abstract class BaseEntity
{
    public Guid Id { get; protected set; } = Guid.NewGuid();
}
