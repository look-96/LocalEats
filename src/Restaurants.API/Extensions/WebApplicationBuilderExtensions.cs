using Microsoft.AspNetCore.RateLimiting;
using Microsoft.OpenApi.Models;
using System.Threading.RateLimiting;
using Restaurants.API.Middlewares;
using Restaurants.API.Services;
using Restaurants.API.Security;
using Restaurants.Application.User;
using Serilog;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace Restaurants.API.Extensions;

public static class WebApplicationBuilderExtensions
{
    public static void AddPresentation(this WebApplicationBuilder builder)
    {
        builder.WebHost.ConfigureKestrel(options => options.Limits.MaxRequestBodySize = 1024 * 1024);
        builder.Services.AddAuthentication();
        builder.Services.ConfigureApplicationCookie(options =>
        {
            options.Cookie.Name = "__Host-LocalEats.Auth";
            options.Cookie.HttpOnly = true;
            options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
            options.Cookie.SameSite = SameSiteMode.Strict;
            options.Cookie.Path = "/";
            options.Cookie.IsEssential = true;
            options.SlidingExpiration = true;
            options.ExpireTimeSpan = TimeSpan.FromHours(8);
        });
        builder.Services.AddAntiforgery(options =>
        {
            options.HeaderName = "X-CSRF-TOKEN";
            options.Cookie.Name = "__Host-LocalEats.Antiforgery";
            options.Cookie.HttpOnly = true;
            options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
            options.Cookie.SameSite = SameSiteMode.Strict;
            options.Cookie.Path = "/";
            options.Cookie.IsEssential = true;
        });
        builder.Services.AddHttpContextAccessor();
        builder.Services.AddScoped<IUserContext, HttpUserContext>();
        builder.Services.AddScoped<IAdminCredentialVerifier, AdminCredentialVerifier>();
        builder.Services.AddScoped<DestructiveCommandAuthorizationFilter>();
        builder.Services.AddControllers(options =>
            options.Conventions.Add(new ProtectDeleteActionsConvention()));
        builder.Services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
            options.AddPolicy("IdentityByIp", httpContext =>
                RateLimitPartition.GetFixedWindowLimiter(
                    partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                    factory: _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = 10,
                        Window = TimeSpan.FromMinutes(1),
                        QueueLimit = 0,
                        AutoReplenishment = true
                    }));
        });
        builder.Services.AddOutputCache(options => options.AddPolicy("PublicCatalog", policy =>
            policy.Expire(TimeSpan.FromMinutes(2))
                .SetVaryByQuery("*")
                .Tag("public-catalog")));
        builder.Services.AddCors(options => options.AddPolicy("RestaurantsClient", policy =>
        {
            var origins = builder.Configuration.GetSection("AllowedOrigins").Get<string[]>() ?? [];
            if (origins.Length > 0)
            {
                policy.WithOrigins(origins).AllowAnyHeader().AllowAnyMethod().AllowCredentials();
            }
        }));
        builder.Services.AddSwaggerGen(c =>
        {
            c.AddSecurityDefinition("bearerAuth", new OpenApiSecurityScheme
            {
                Type = SecuritySchemeType.Http,
                Scheme = "Bearer"
            });

            c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference {Type = ReferenceType.SecurityScheme, Id = "bearerAuth"}
            },
            []
        }
    });
        });

        builder.Services.AddEndpointsApiExplorer();

        builder.Services.AddScoped<ErrorHandlingMiddleware>();
        builder.Services.AddScoped<RequestTimeLoggingMiddleware>();

        builder.Host.UseSerilog((context, configuration) =>
        configuration
        .ReadFrom.Configuration(context.Configuration)
        );
    }
}
