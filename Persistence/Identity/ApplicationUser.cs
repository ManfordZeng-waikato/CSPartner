using Microsoft.AspNetCore.Identity;

namespace Persistence.Identity;

/// <summary>
/// 账号实体（只负责登录、权限）
/// </summary>
public class ApplicationUser : IdentityUser<Guid>
{
}
