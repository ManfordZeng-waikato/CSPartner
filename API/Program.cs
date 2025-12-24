using Application.Behaviors;
using Application.Interfaces.Services;
using Infrastructure;
using Infrastructure.Identity;
using Infrastructure.Persistence.Context;
using Infrastructure.Persistence.Identity;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Text;
using Resend;

var builder = WebApplication.CreateBuilder(args);

// Add Infrastructure services (DbContext, Storage)
builder.Services.AddInfrastructure(builder.Configuration);

// Add MediatR
builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssembly(typeof(Application.Behaviors.TransactionBehavior<,>).Assembly);
});
builder.Services.AddHttpClient<ResendClient>();
builder.Services.Configure<ResendClientOptions>(opt =>
{
  opt.ApiToken = builder.Configuration["Resend:ApiToken"]!;
});
builder.Services.AddTransient<IResend,ResendClient>();

// Add MediatR Pipeline Behaviors
builder.Services.AddScoped(typeof(IPipelineBehavior<,>), typeof(TransactionBehavior<,>));

// CORS configuration - only needed when frontend is served separately
// When frontend is served from wwwroot, CORS is not needed (same origin)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReactClient", policy =>
    {
        if (builder.Environment.IsDevelopment())
        {
            // In development, allow separate frontend dev server
            policy.WithOrigins("http://localhost:3000", "https://localhost:3000")
                .AllowAnyMethod()
                .AllowAnyHeader()
                .AllowCredentials();
        }
        else
        {
            // In production, if frontend is served from wwwroot, CORS is not needed
            // This policy can be removed or adjusted based on your deployment scenario
            policy.AllowAnyOrigin()
                .AllowAnyMethod()
                .AllowAnyHeader();
        }
    });
});

// Register email services
builder.Services.AddTransient<IEmailSender<ApplicationUser>, EmailSenderService>();
builder.Services.AddTransient<Application.Interfaces.Services.IEmailService, Infrastructure.Identity.EmailService>();

builder.Services.AddIdentity<ApplicationUser, ApplicationRole>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 8;
    options.Password.RequireUppercase = true;
    options.User.RequireUniqueEmail = true;
    options.SignIn.RequireConfirmedEmail = true;
})
.AddEntityFrameworkStores<AppDbContext>()
.AddDefaultTokenProviders();

// Configure JWT Authentication
var jwtSecretKey = builder.Configuration["Jwt:SecretKey"];
var jwtIssuer = builder.Configuration["Jwt:Issuer"];
var jwtAudience = builder.Configuration["Jwt:Audience"];

// Verify all required JWT configuration values are present
if (!string.IsNullOrEmpty(jwtSecretKey) &&
    !string.IsNullOrEmpty(jwtIssuer) &&
    !string.IsNullOrEmpty(jwtAudience))
{
    builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtIssuer,
            ValidAudience = jwtAudience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecretKey)),
            ClockSkew = TimeSpan.Zero,
            NameClaimType = JwtRegisteredClaimNames.Sub,
            RoleClaimType = ClaimTypes.Role
        };

        // Support JWT token in query string for SignalR
        options.Events = new Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                var accessToken = context.Request.Query["access_token"];
                var path = context.HttpContext.Request.Path;
                if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/hubs"))
                {
                    context.Token = accessToken;
                }
                return Task.CompletedTask;
            }
        };
    });
}
else
{
    throw new InvalidOperationException(
        "JWT authentication requires all of the following configuration values: " +
        "Jwt:SecretKey, Jwt:Issuer, and Jwt:Audience. " +
        "Please check your appsettings.json file.");
}

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
        options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
    });

// Add SignalR
builder.Services.AddSignalR();

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

// Authorization is configured per-controller/action using [Authorize] or [AllowAnonymous] attributes
// Static files and SPA routes are served without authentication requirements
builder.Services.AddAuthorization();

// Static files will be served from wwwroot directory (no additional service registration needed)

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

// Serve static files from wwwroot (frontend build output)
// Placed before UseRouting for better performance - static files are served directly
app.UseDefaultFiles();
app.UseStaticFiles();

app.UseRouting();

// CORS must be after UseRouting and before UseAuthorization and MapControllers
// Only use CORS in development when frontend is served separately
if (app.Environment.IsDevelopment())
{
    app.UseCors("AllowReactClient");
}

app.UseAuthentication();
app.UseAuthorization();

// Map API controllers and SignalR Hub - endpoint mapping happens after static files
app.MapControllers();
app.MapHub<API.SignalR.CommentHub>("/api/hubs/comments");
app.MapFallbackToFile("index.html");

// SPA fallback: serve index.html for all non-API routes
// This must be last in the middleware pipeline (except for error handling)


using var scope = app.Services.CreateScope();
var services = scope.ServiceProvider;

try
{
    var logger = services.GetRequiredService<ILogger<Program>>();
    var db = services.GetRequiredService<AppDbContext>();

    // ✅ Auto migration
    await db.Database.MigrateAsync();

    // ✅ Seed (with switch)
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

app.Run();
