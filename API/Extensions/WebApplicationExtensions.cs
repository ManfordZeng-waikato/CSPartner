using Infrastructure.Persistence.Context;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.EntityFrameworkCore;

namespace API.Extensions;

public static class WebApplicationExtensions
{
    /// <summary>
    /// Configure the HTTP request pipeline middleware
    /// </summary>
    public static WebApplication ConfigureMiddlewarePipeline(this WebApplication app)
    {
        // Swagger UI in development
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "CSPartner API V1");
                c.RoutePrefix = "swagger";
                c.DocExpansion(Swashbuckle.AspNetCore.SwaggerUI.DocExpansion.List);
            });
        }

        // Global exception handling (must be early in pipeline)
        app.UseMiddleware<API.Middleware.ExceptionHandlingMiddleware>();

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

        // Static files (frontend build output) with caching
        app.UseDefaultFiles();
        app.UseStaticFiles(new StaticFileOptions
        {
            OnPrepareResponse = ctx =>
            {
                // Cache static files for 1 year (images, fonts, JS, CSS)
                const int durationInSeconds = 60 * 60 * 24 * 365; // 1 year
                ctx.Context.Response.Headers.Append(
                    "Cache-Control",
                    $"public,max-age={durationInSeconds}");
            }
        });

        // Routing
        app.UseRouting();
        
        // Response caching (must be after UseRouting, before UseAuthentication)
        app.UseResponseCaching();

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

            // Auto migration - only for relational databases
            if (db.Database.IsRelational())
            {
                await db.Database.MigrateAsync();
                logger.LogInformation("Database migration completed.");
            }

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

