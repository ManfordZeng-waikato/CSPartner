using Domain.Users;
using Domain.Videos;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Infrastructure.Persistence.Context;
using Infrastructure.Persistence.Identity;

namespace API.Seed;

public static class DemoSeeder
{
    public static async Task SeedAsync(IServiceProvider services, IConfiguration config)
    {
        using var scope = services.CreateScope();

        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<ApplicationRole>>();

        // 0) 幂等开关：只在你启用时才跑
        // 也可以把这个判断放 Program.cs 外层
        var enabled = config.GetValue<bool>("Seed:DemoData");
        if (!enabled) return;

        // 1) Roles（可选）
        string[] roles = ["User", "Admin"];
        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
                await roleManager.CreateAsync(new ApplicationRole { Name = role });
        }

        // 2) 确保 uploader 用户存在
        var demoEmail = "demo@highlighthub.local";
        var demoUser = await userManager.FindByEmailAsync(demoEmail);

        if (demoUser is null)
        {
            demoUser = new ApplicationUser
            {
                UserName = demoEmail,
                Email = demoEmail,
                EmailConfirmed = true
            };

            // 密码不要硬编码，建议环境变量配置
            var pwd = config["Seed:DemoUserPassword"] ?? "Demo@12345";
            var result = await userManager.CreateAsync(demoUser, pwd);
            if (!result.Succeeded)
                throw new Exception(string.Join(", ", result.Errors.Select(e => e.Description)));

            await userManager.AddToRoleAsync(demoUser, "User");

            // 同步创建 UserProfile（你的业务资料表）
            var profile = new UserProfile(demoUser.Id);
            profile.Update("DemoPlayer", "Seeded demo profile", null,
                "https://steamcommunity.com/profiles/76561198828107858/",
                "https://www.faceit.com/en/players/Manf0rd");

            db.UserProfiles.Add(profile);
            await db.SaveChangesAsync();
        }

        // 3) 插入 demo 视频（幂等：用 VideoUrl 去重）
        var demoVideos = new List<HighlightVideo>
        {
            new(demoUser.Id, "CS2 1v3 Clutch",
                "https://pub-bda0f9dd33824f6bbb05c0eed9da4d44.r2.dev/highlight-BjNidspXxfeoDSzXtkjuNUjrYd.mp4", "Demo highlight"
                ),

            new(demoUser.Id, "FACEIT Entry Frag",
                "https://pub-bda0f9dd33824f6bbb05c0eed9da4d44.r2.dev/highlight-BnnHntrpFdmsqHQVHcgBuCejfq.mp4", "Demo highlight"
                ),
            new(demoUser.Id, "3 Kills",
                "https://pub-bda0f9dd33824f6bbb05c0eed9da4d44.r2.dev/highlight-iSGOUcIiMUEfdSONmiInQCnUy.mp4", "Demo highlight"
                ),
            new(demoUser.Id, "4 Kills",
                "https://pub-bda0f9dd33824f6bbb05c0eed9da4d44.r2.dev/highlight-cpwfgHvMjOjrdscmvXUipxJAW.mp4", "Demo highlight"
                ),
            new(demoUser.Id, "DEMO",
                "https://pub-bda0f9dd33824f6bbb05c0eed9da4d44.r2.dev/highlight-PPmluanzFFBeumBAJDqWXdhuT.mp4", "Demo highlight"
                ),
        };

        // 只插入不存在的（以 VideoUrl 为唯一标F识）
        var existingUrls = await db.Videos.Select(v => v.VideoUrl).ToListAsync();
        var toInsert = demoVideos.Where(v => !existingUrls.Contains(v.VideoUrl)).ToList();

        if (toInsert.Count > 0)
        {
            db.Videos.AddRange(toInsert);
            await db.SaveChangesAsync();
        }
    }
}
