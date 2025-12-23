using Microsoft.AspNetCore.Identity;

namespace Infrastructure.Persistence.Identity;

/// <summary>
/// Account entity (only responsible for login and permissions)
/// </summary>
public class ApplicationUser : IdentityUser<Guid>
{
}
