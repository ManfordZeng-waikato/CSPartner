using System.Collections.Generic;
using API.Tests.Helpers;
using Application.Common.Interfaces;
using Infrastructure.Persistence.Context;
using Application.Features.Videos.Commands.IncreaseViewCount;
using MediatR;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.AspNetCore.Mvc.Testing;

namespace API.Tests.Helpers;

public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Test");

        builder.ConfigureAppConfiguration((_, config) =>
        {
            var settings = new Dictionary<string, string?>
            {
                ["ConnectionStrings:Default"] = "Server=(localdb)\\mssqllocaldb;Database=CSPartner_Test;Trusted_Connection=True;",
                ["Jwt:SecretKey"] = "TestSecretKey-For-Integration-Only-1234567890",
                ["Jwt:Issuer"] = "TestIssuer",
                ["Jwt:Audience"] = "TestAudience",
                ["Authentication:Github:ClientId"] = "test-client-id",
                ["Authentication:Github:ClientSecret"] = "test-client-secret",
                ["Resend:ApiToken"] = "test-resend-token",
                ["OpenAI:ApiKey"] = "test-openai-key",
                ["Seed:DemoData"] = "false"
            };

            config.AddInMemoryCollection(settings);
        });

        builder.ConfigureServices(services =>
        {
            services.RemoveAll(typeof(DbContextOptions<AppDbContext>));
            services.RemoveAll(typeof(DbContextOptions));
            var toRemove = services
                .Where(d =>
                    d.ServiceType.IsGenericType &&
                    d.ServiceType.GetGenericTypeDefinition().Name == "IDbContextOptionsConfiguration`1" &&
                    d.ServiceType.GenericTypeArguments[0] == typeof(AppDbContext))
                .ToList();
            foreach (var descriptor in toRemove)
            {
                services.Remove(descriptor);
            }
            services.RemoveAll(typeof(AppDbContext));
            services.RemoveAll(typeof(IApplicationDbContext));
            services.RemoveAll(typeof(IAuthService));

            services.AddDbContext<AppDbContext>(options =>
                options.UseInMemoryDatabase("ApiTestsDb"));
            services.AddScoped<IApplicationDbContext>(sp => sp.GetRequiredService<AppDbContext>());
            services.AddSingleton<IAuthService, FakeAuthService>();

            services.RemoveAll(typeof(IPipelineBehavior<,>));
            services.AddScoped(typeof(IPipelineBehavior<,>), typeof(TestCommandSaveChangesBehavior<,>));

            services.RemoveAll(typeof(IRequestHandler<IncreaseViewCountCommand, Unit>));
            services.AddScoped<IRequestHandler<IncreaseViewCountCommand, Unit>, TestIncreaseViewCountHandler>();

            services.AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme = TestAuthDefaults.Scheme;
                    options.DefaultChallengeScheme = TestAuthDefaults.Scheme;
                })
                .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>(TestAuthDefaults.Scheme, _ => { });

            services.AddAuthorization();
        });
    }
}
