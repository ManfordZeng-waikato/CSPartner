using API.Extensions;
using API.Seed;
using Infrastructure;
using Microsoft.AspNetCore.HttpOverrides;

var builder = WebApplication.CreateBuilder(args);

// Trust X-Forwarded-* headers from Railway/Cloudflare so Request.Scheme is https
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    options.KnownNetworks.Clear();
    options.KnownProxies.Clear();
});

// Railway/container: listen on PORT (default 8080), all interfaces (0.0.0.0)
var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
builder.WebHost.UseUrls($"http://0.0.0.0:{port}");

// Configure services
builder.Services
    .AddDataProtectionConfiguration(builder.Configuration, builder.Environment)
    .AddInfrastructure(builder.Configuration)
    .AddMediatRConfiguration()
    .AddEmailServices(builder.Configuration)
    .AddCorsConfiguration(builder.Configuration, builder.Environment)
    .AddIdentityConfiguration()
    .AddAuthenticationConfiguration(builder.Configuration, builder.Environment)
    .AddRateLimiting()
    .AddApiServices(builder.Configuration);

var app = builder.Build();

// Seed-only mode: dotnet run --project API -- seed (runs DemoSeeder and exits)
if (args is ["seed"])
{
    var config = app.Services.GetRequiredService<IConfiguration>();
    await DemoSeeder.SeedAsync(app.Services, config);
    Console.WriteLine("Seed completed.");
    return;
}

// Configure middleware pipeline
app.ConfigureMiddlewarePipeline();

// Initialize database
await app.InitializeDatabaseAsync();

app.Run();

public partial class Program { }
