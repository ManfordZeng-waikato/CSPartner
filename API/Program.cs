using API.Extensions;
using Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// Configure services
builder.Services
    .AddDataProtectionConfiguration(builder.Configuration, builder.Environment)
    .AddInfrastructure(builder.Configuration)
    .AddMediatRConfiguration()
    .AddEmailServices(builder.Configuration)
    .AddCorsConfiguration(builder.Environment)
    .AddIdentityConfiguration()
    .AddAuthenticationConfiguration(builder.Configuration, builder.Environment)
    .AddRateLimiting()
    .AddApiServices(builder.Configuration);

var app = builder.Build();

// Configure middleware pipeline
app.ConfigureMiddlewarePipeline();

// Initialize database
await app.InitializeDatabaseAsync();

app.Run();
