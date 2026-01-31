using Domain.Users;
using FluentAssertions;

namespace Domain.Tests;

public class UserProfileTests
{
    [Fact]
    public void Update_normalizes_fields_and_urls()
    {
        var profile = new UserProfile(Guid.NewGuid());
        var longName = new string('n', 80);
        var longBio = new string('b', 600);

        profile.Update(
            $"  {longName}  ",
            $"  {longBio}  ",
            "  https://avatar  ",
            "  https://steam  ",
            "  https://faceit  ");

        profile.DisplayName.Should().Be(longName[..50]);
        profile.Bio.Should().Be(longBio[..500]);
        profile.AvatarUrl.Should().Be("https://avatar");
        profile.SteamProfileUrl.Should().Be("https://steam");
        profile.FaceitProfileUrl.Should().Be("https://faceit");
    }
}
