using Application.Behaviors;
using Application.Interfaces.Services;
using Infrastructure;
using Infrastructure.Identity;
using Infrastructure.Persistence.Context;
using Infrastructure.Persistence.Identity;
using MediatR;
using Microsoft.AspNetCore.Authentication.Cookies;
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
            "JWT authentication requires configuration: Jwt:SecretKey, Jwt:Issuer, Jwt:Audience");
    }

    // External login Cookie scheme (only used during GitHub OAuth callback)
    const string ExternalScheme = "External";

    services.AddAuthentication(options =>
    {
        // API defaults to JWT
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

        // SignalR reads token from query string
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
    .AddCookie(ExternalScheme, options =>
    {
        // This Cookie only exists briefly during OAuth callback
        options.Cookie.Name = "__Host-cspartner-external";
        options.Cookie.HttpOnly = true;
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
        options.Cookie.SameSite = SameSiteMode.None; // Required for cross-port/cross-domain scenarios
        options.ExpireTimeSpan = TimeSpan.FromMinutes(5);
        options.SlidingExpiration = false;
    })
    .AddGitHub(options =>
    {
        var githubClientId = configuration["Authentication:Github:ClientId"];
        var githubClientSecret = configuration["Authentication:Github:ClientSecret"];

        if (string.IsNullOrEmpty(githubClientId) || string.IsNullOrEmpty(githubClientSecret))
        {
            throw new InvalidOperationException("GitHub OAuth requires Authentication:Github:ClientId and ClientSecret to be configured");
        }

        options.ClientId = githubClientId;
        options.ClientSecret = githubClientSecret;

        // Write GitHub OAuth login result to External Cookie
        options.SignInScheme = ExternalScheme;

        // OAuth callback path (must be configured in GitHub App)
        options.CallbackPath = "/signin-github";

        // Request email permission (may still not be available, need fallback)
        options.Scope.Add("user:email");

        // Save access_token for fetching email from GitHub API if needed
        options.SaveTokens = true;

        // Correlation cookie: must be None + Secure for cross-site/cross-port scenarios
        options.CorrelationCookie.SameSite = SameSiteMode.None;
        options.CorrelationCookie.SecurePolicy = CookieSecurePolicy.Always;
        options.CorrelationCookie.HttpOnly = true;

        // Redirect to business logic handler after successful login
        options.Events.OnTicketReceived = context =>
        {
            // Note: ReturnUri should be set in GitHubLogin method instead
            return Task.CompletedTask;
        };

        // Log real error on failure (critical to avoid "Unknown error")
        options.Events.OnRemoteFailure = context =>
        {
            var logger = context.HttpContext.RequestServices
                .GetRequiredService<ILoggerFactory>()
                .CreateLogger("GitHubOAuth");

            logger.LogError(context.Failure, "GitHub OAuth remote authentication failed");

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

