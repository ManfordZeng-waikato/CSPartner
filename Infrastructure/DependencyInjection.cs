using Application.Common.Interfaces;
using Infrastructure.AI;
using Infrastructure.Identity;
using Infrastructure.Persistence.Context;
using Infrastructure.Storage;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // HttpContextAccessor for CurrentUserService
        services.AddHttpContextAccessor();

        // Database Context
        services.AddDbContext<AppDbContext>(options =>
        {
            options.UseSqlServer(configuration.GetConnectionString("Default"));
        });

        // Register IApplicationDbContext
        services.AddScoped<IApplicationDbContext>(provider => provider.GetRequiredService<AppDbContext>());

        // Current User Service
        services.AddScoped<ICurrentUserService, CurrentUserService>();

        // Infrastructure Services
        services.AddScoped<IStorageService, R2StorageService>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IJwtService, JwtService>();

        // Token Blacklist Service (singleton for in-memory cache)
        services.AddSingleton<Application.Common.Interfaces.ITokenBlacklistService, TokenBlacklistService>();

        // AI Services
        services.AddHttpClient<IAiVideoService, OpenAiVideoService>((sp, client) =>
     {
         var config = sp.GetRequiredService<IConfiguration>();
         var apiKey = config["OpenAI:ApiKey"] ?? throw new InvalidOperationException("OpenAI:ApiKey is missing.");

         client.BaseAddress = new Uri("https://api.openai.com/");
         client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", apiKey);
         client.Timeout = TimeSpan.FromSeconds(20);
     });


        return services;
    }
}
