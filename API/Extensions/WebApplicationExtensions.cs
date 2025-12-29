using Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace API.Extensions;

public static class WebApplicationExtensions
{
    /// <summary>
    /// Configure the HTTP request pipeline middleware
    /// </summary>
    public static WebApplication ConfigureMiddlewarePipeline(this WebApplication app)
    {
        // OpenAPI in development
        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();
        }

        // Domain redirect middleware
        app.Use(async (context, next) =>
        {
            if (context.Request.Host.Host.Equals("cspartner.org", StringComparison.OrdinalIgnoreCase))
            {
                var newUrl = $"https://www.cspartner.org{context.Request.Path}{context.Request.QueryString}";
                context.Response.Redirect(newUrl, permanent: true);
                return;
            }

            await next();
        });

        // HTTPS redirection
        app.UseHttpsRedirection();

        // Static files (frontend build output)
        app.UseDefaultFiles();
        app.UseStaticFiles();

        // Routing
        app.UseRouting();

        // Rate limiting (must be after UseRouting)
        app.UseRateLimiter();

        // CORS (only in development when frontend is served separately)
        if (app.Environment.IsDevelopment())
        {
            app.UseCors("AllowReactClient");
        }

        // Authentication & Authorization
        app.UseAuthentication();
        app.UseAuthorization();

        // API endpoints
        app.MapControllers();
        app.MapHub<API.SignalR.CommentHub>("/api/hubs/comments");

        // SPA fallback (must be last)
        app.MapFallbackToFile("index.html");

        return app;
    }

    /// <summary>
    /// Initialize database (migration and seeding)
    /// </summary>
    public static async Task InitializeDatabaseAsync(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var services = scope.ServiceProvider;

        try
        {
            var logger = services.GetRequiredService<ILogger<Program>>();
            var db = services.GetRequiredService<AppDbContext>();

            // Auto migration
            await db.Database.MigrateAsync();

            // Seed (with switch)
            await API.Seed.DemoSeeder.SeedAsync(app.Services, app.Configuration);

            logger.LogInformation("Database migrated & demo data seeded.");
        }
        catch (Exception ex)
        {
            var logger = services.GetRequiredService<ILogger<Program>>();
            logger.LogError(ex, "Database migration/seed failed.");
            // In production, you can choose to: directly throw to restart the container; or continue startup
            // throw;
        }
    }
}

