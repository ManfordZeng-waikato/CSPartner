using Application.Common.Interfaces;
using Infrastructure.Identity;
using Infrastructure.Persistence.Context;
using Infrastructure.Persistence.Identity;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Tests.Helpers;

public sealed class AuthServiceTestScope : IDisposable
{
    public ServiceProvider Provider { get; }

    private AuthServiceTestScope(ServiceProvider provider)
    {
        Provider = provider;
    }

    public static AuthServiceTestScope Create()
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:SecretKey"] = "TestSecretKey-For-Integration-Only-1234567890",
                ["Jwt:Issuer"] = "TestIssuer",
                ["Jwt:Audience"] = "TestAudience",
                ["ClientApp:ClientUrl"] = "https://client.test"
            })
            .Build();

        var services = new ServiceCollection();
        services.AddSingleton<IConfiguration>(config);
        services.AddLogging();
        services.AddHttpContextAccessor();
        services.AddAuthentication();

        var dbName = $"AuthServiceTests-{Guid.NewGuid()}";
        services.AddDbContext<AppDbContext>(options =>
            options.UseInMemoryDatabase(dbName));

        services.AddIdentity<ApplicationUser, ApplicationRole>()
            .AddEntityFrameworkStores<AppDbContext>()
            .AddDefaultTokenProviders();

        services.AddScoped<IApplicationDbContext>(sp => sp.GetRequiredService<AppDbContext>());
        services.AddSingleton<IEmailService, FakeEmailService>();
        services.AddSingleton<ITokenBlacklistService, FakeTokenBlacklistService>();
        services.AddSingleton<ICurrentUserService, FakeCurrentUserService>();
        services.AddSingleton<IJwtService, JwtService>();
        services.AddTransient<AuthService>();

        var provider = services.BuildServiceProvider();

        return new AuthServiceTestScope(provider);
    }

    public void Dispose()
    {
        Provider.Dispose();
    }
}
