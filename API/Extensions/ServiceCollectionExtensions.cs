using Application.Behaviors;
using Application.Interfaces.Services;
using Infrastructure;
using Infrastructure.Identity;
using Infrastructure.Persistence.Context;
using Infrastructure.Persistence.Identity;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Resend;
using AspNet.Security.OAuth.GitHub;
using Azure.Identity;
using Azure.Security.KeyVault.Keys;
using Azure.Storage.Blobs;
using Azure.Extensions.AspNetCore.DataProtection.Blobs;

namespace API.Extensions;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Configure data protection for OAuth state management
    /// </summary>
    public static IServiceCollection AddDataProtectionConfiguration(
        this IServiceCollection services,
        IConfiguration configuration,
        IWebHostEnvironment environment)
    {
        var keyVaultUri = configuration["DataProtection:KeyVaultUri"];
        var blobStorageConnectionString = configuration["DataProtection:BlobStorageConnectionString"];
        var useAzureStorage = !string.IsNullOrWhiteSpace(keyVaultUri) &&
                              !string.IsNullOrWhiteSpace(blobStorageConnectionString) &&
                              !environment.IsDevelopment();

        if (useAzureStorage)
        {
            try
            {
                var credential = new DefaultAzureCredential();
                var keyVaultUriObj = new Uri(keyVaultUri!);

                var keyName = configuration["DataProtection:KeyName"] ?? "DataProtection-Keys";
                var keyUri = new Uri($"{keyVaultUriObj}keys/{keyName}");

                var blobServiceClient = new BlobServiceClient(blobStorageConnectionString);
                var containerName = configuration["DataProtection:ContainerName"] ?? "dataprotection-keys";
                var blobName = configuration["DataProtection:BlobName"] ?? "keys.xml";
                var blobContainerClient = blobServiceClient.GetBlobContainerClient(containerName);

                blobContainerClient.CreateIfNotExists();
                var blobClient = blobContainerClient.GetBlobClient(blobName);

                services.AddDataProtection()
                    .PersistKeysToAzureBlobStorage(blobClient)
                    .ProtectKeysWithAzureKeyVault(keyUri, credential)
                    .SetApplicationName("CSPartner");
            }
            catch (Exception ex)
            {
                var loggerFactory = LoggerFactory.Create(b => b.AddConsole());
                var logger = loggerFactory.CreateLogger("DataProtection");
                logger.LogError(ex, "Failed to configure Azure storage for data protection, falling back to file system");

                services.AddDataProtection()
                    .PersistKeysToFileSystem(new DirectoryInfo(Path.Combine(environment.ContentRootPath, "DataProtection-Keys")))
                    .SetApplicationName("CSPartner");
            }
        }
        else
        {
            services.AddDataProtection()
                .PersistKeysToFileSystem(new DirectoryInfo(Path.Combine(environment.ContentRootPath, "DataProtection-Keys")))
                .SetApplicationName("CSPartner");
        }

        return services;
    }

    /// <summary>
    /// Configure MediatR and pipeline behaviors
    /// </summary>
    public static IServiceCollection AddMediatRConfiguration(this IServiceCollection services)
    {
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(typeof(TransactionBehavior<,>).Assembly);
        });

        services.AddScoped(typeof(IPipelineBehavior<,>), typeof(TransactionBehavior<,>));

        return services;
    }

    /// <summary>
    /// Configure email services (Resend)
    /// </summary>
    public static IServiceCollection AddEmailServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddHttpClient<ResendClient>();
        services.Configure<ResendClientOptions>(opt =>
        {
            var apiToken = configuration["Resend:ApiToken"];
            if (string.IsNullOrWhiteSpace(apiToken))
            {
                throw new InvalidOperationException(
                    "Resend API token is not configured. Please set 'Resend:ApiToken' in appsettings.json or environment variables.");
            }
            opt.ApiToken = apiToken;
        });
        services.AddTransient<IResend, ResendClient>();

        // Register email services
        services.AddTransient<IEmailSender<ApplicationUser>, EmailSenderService>();
        services.AddTransient<Application.Interfaces.Services.IEmailService, EmailSenderService>();

        return services;
    }

    /// <summary>
    /// Configure CORS policy
    /// </summary>
    public static IServiceCollection AddCorsConfiguration(
        this IServiceCollection services,
        IWebHostEnvironment environment)
    {
        services.AddCors(options =>
        {
            options.AddPolicy("AllowReactClient", policy =>
            {
                if (environment.IsDevelopment())
                {
                    policy.WithOrigins("http://localhost:3000", "https://localhost:3000")
                        .AllowAnyMethod()
                        .AllowAnyHeader()
                        .AllowCredentials();
                }
                else
                {
                    policy.AllowAnyOrigin()
                        .AllowAnyMethod()
                        .AllowAnyHeader();
                }
            });
        });

        return services;
    }

    /// <summary>
    /// Configure ASP.NET Core Identity
    /// </summary>
    public static IServiceCollection AddIdentityConfiguration(this IServiceCollection services)
    {
        services.AddIdentity<ApplicationUser, ApplicationRole>(options =>
        {
            options.Password.RequireDigit = true;
            options.Password.RequiredLength = 8;
            options.Password.RequireUppercase = true;
            options.Password.RequireNonAlphanumeric = true;
            options.User.RequireUniqueEmail = true;
            options.SignIn.RequireConfirmedEmail = true;
        })
        .AddEntityFrameworkStores<AppDbContext>()
        .AddDefaultTokenProviders();

        return services;
    }

    /// <summary>
    /// Configure JWT and GitHub OAuth authentication
    /// </summary>
    public static IServiceCollection AddAuthenticationConfiguration(
        this IServiceCollection services,
        IConfiguration configuration,
        IWebHostEnvironment environment)
    {
        var jwtSecretKey = configuration["Jwt:SecretKey"];
        var jwtIssuer = configuration["Jwt:Issuer"];
        var jwtAudience = configuration["Jwt:Audience"];

        if (string.IsNullOrEmpty(jwtSecretKey) ||
            string.IsNullOrEmpty(jwtIssuer) ||
            string.IsNullOrEmpty(jwtAudience))
        {
            throw new InvalidOperationException(
                "JWT authentication requires all of the following configuration values: " +
                "Jwt:SecretKey, Jwt:Issuer, and Jwt:Audience. " +
                "Please check your appsettings.json file.");
        }

        services.AddAuthentication(options =>
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
            options.Events = new JwtBearerEvents
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
        })
        .AddGitHub(options =>
        {
            var githubClientId = configuration["Authentication:Github:ClientId"];
            var githubClientSecret = configuration["Authentication:Github:ClientSecret"];

            if (string.IsNullOrEmpty(githubClientId) || string.IsNullOrEmpty(githubClientSecret))
            {
                throw new InvalidOperationException(
                    "GitHub OAuth requires 'Authentication:Github:ClientId' and 'Authentication:Github:ClientSecret' " +
                    "to be configured in appsettings.json or environment variables.");
            }

            options.ClientId = githubClientId;
            options.ClientSecret = githubClientSecret;
            // Use default callback path - OAuth middleware handles this
            // After successful callback, redirect to our controller handler
            options.CallbackPath = "/signin-github";
            options.Scope.Add("user:email");
            options.SaveTokens = true;

            // Configure cookie settings for OAuth state management
            // The OAuth correlation cookie stores the state parameter used to prevent CSRF attacks
            // For localhost development with different ports, we need None + Secure for cross-origin cookies
            if (environment.IsDevelopment())
            {
                // Development: Use None + Always Secure for localhost cross-port scenarios
                // Both frontend (3000) and backend (5001) use HTTPS, so Secure is required
                options.CorrelationCookie.SameSite = Microsoft.AspNetCore.Http.SameSiteMode.None;
                options.CorrelationCookie.SecurePolicy = Microsoft.AspNetCore.Http.CookieSecurePolicy.Always;
                options.CorrelationCookie.HttpOnly = true;
            }
            else
            {
                // Production: Use None + Always Secure for cross-site scenarios
                options.CorrelationCookie.SameSite = Microsoft.AspNetCore.Http.SameSiteMode.None;
                options.CorrelationCookie.SecurePolicy = Microsoft.AspNetCore.Http.CookieSecurePolicy.Always;
                options.CorrelationCookie.HttpOnly = true;
            }

            // Handle successful OAuth callback - redirect to our controller
            options.Events.OnTicketReceived = context =>
            {
                // OAuth middleware has successfully authenticated
                // Redirect to our controller handler which will extract claims and create JWT
                context.Response.Redirect("/api/account/github-callback");
                context.HandleResponse();
                return Task.CompletedTask;
            };

            // Handle OAuth failures
            options.Events.OnRemoteFailure = context =>
            {
                var clientUrl = configuration["ClientApp:ClientUrl"] ?? "https://localhost:3000";
                context.Response.Redirect($"{clientUrl}/login?error=github_auth_failed");
                context.HandleResponse();
                return Task.CompletedTask;
            };
        });

        return services;
    }

    /// <summary>
    /// Configure controllers, SignalR, and OpenAPI
    /// </summary>
    public static IServiceCollection AddApiServices(this IServiceCollection services)
    {
        services.AddControllers()
            .AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
                options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
            });

        services.AddSignalR();
        services.AddOpenApi();
        services.AddAuthorization();

        return services;
    }
}

