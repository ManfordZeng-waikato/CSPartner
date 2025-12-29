using Application.Common.Interfaces;
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

        return services;
    }
}
