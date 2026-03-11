using API.Extensions;
using API.Seed;
using Infrastructure;

var builder = WebApplication.CreateBuilder(args);

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
