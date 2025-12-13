using Domain.Common;

namespace Domain.Users;

/// <summary>
/// 用户业务资料（Identity User 的扩展）
/// </summary>
public class UserProfile : AuditableEntity
{
    public Guid UserId { get; private set; }   // 对应 AspNetUsers.Id

    public string? DisplayName { get; private set; }
    public string? Bio { get; private set; }
    public string? AvatarUrl { get; private set; }
    public string? SteamProfileUrl { get; private set; }
    public string? FaceitProfileUrl { get; private set; }

    private UserProfile() { }

    public UserProfile(Guid userId)
    {
        UserId = userId;
    }

    public void Update(
        string? displayName,
        string? bio,
        string? avatarUrl,
        string? steamUrl,
        string? faceitUrl)
    {
        DisplayName = Normalize(displayName, 50);
        Bio = Normalize(bio, 500);
        AvatarUrl = NormalizeUrl(avatarUrl);
        SteamProfileUrl = NormalizeUrl(steamUrl);
        FaceitProfileUrl = NormalizeUrl(faceitUrl);

        Touch();
    }

    private static string? Normalize(string? input, int max)
    {
        if (string.IsNullOrWhiteSpace(input)) return null;
        var s = input.Trim();
        return s.Length <= max ? s : s[..max];
    }

    private static string? NormalizeUrl(string? url)
        => string.IsNullOrWhiteSpace(url) ? null : url.Trim();
}
