using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.RateLimiting;
using Restaurants.API.Extensions;
using Restaurants.API.Middlewares;
using Restaurants.API.Security;
using Restaurants.Application.Extensions;
using Restaurants.Domain.Entities;
using Restaurants.Infrastructure.Extensions;
using Restaurants.Infrastructure.Seeders;
using Serilog;

try
{
    var builder = WebApplication.CreateBuilder(args);

    // Add services to the container.

    builder.AddPresentation();

    builder.Services.AddApplication();
    builder.Services.AddInfrastructure(builder.Configuration);


    var app = builder.Build();

    var scope = app.Services.CreateScope();
    var initializer = scope.ServiceProvider.GetRequiredService<IStartupDataInitializer>();

    await initializer.InitializeAsync();

    // Configure the HTTP request pipeline.

    app.UseMiddleware<ErrorHandlingMiddleware>();
    app.UseMiddleware<RequestTimeLoggingMiddleware>();

    app.UseSerilogRequestLogging();

    app.MapGet("/health", () => Results.Ok(new { status = "healthy" })).AllowAnonymous();

    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    if (app.Environment.IsDevelopment())
        app.UseHttpsRedirection();
    app.Use(async (context, next) =>
    {
        context.Response.Headers["X-Content-Type-Options"] = "nosniff";
        context.Response.Headers["X-Frame-Options"] = "DENY";
        context.Response.Headers["Referrer-Policy"] = "strict-origin-when-cross-origin";
        context.Response.Headers["Permissions-Policy"] = "camera=(), microphone=(), geolocation=()";
        if (!app.Environment.IsDevelopment())
            context.Response.Headers["Strict-Transport-Security"] = "max-age=31536000; includeSubDomains";
        context.Response.Headers["Content-Security-Policy"] = "default-src 'self'; base-uri 'self'; object-src 'none'; form-action 'self'; frame-ancestors 'none'; script-src 'self' 'wasm-unsafe-eval'; style-src 'self' 'unsafe-inline' https://fonts.googleapis.com; font-src 'self' data: https://fonts.gstatic.com; img-src 'self' data: https://storage.googleapis.com; connect-src 'self'; upgrade-insecure-requests";
        await next();
    });
    app.UseLocalEatsClientFiles();
    app.UseRouting();

    app.UseCors("RestaurantsClient");
    app.UseRateLimiter();
    app.UseAuthentication();
    app.UseMiddleware<AntiforgeryValidationMiddleware>();

    app.UseOutputCache();

    var identityGroup = app.MapGroup("api/identity").WithTags("Identity");
    identityGroup.RequireRateLimiting("IdentityByIp");
    identityGroup.MapIdentityApi<User>();
    identityGroup.MapPost("logout", async (Microsoft.AspNetCore.Identity.SignInManager<User> signInManager) =>
    {
        await signInManager.SignOutAsync();
        return Results.NoContent();
    }).RequireAuthorization();
    app.MapGet("/api/security/antiforgery", (Microsoft.AspNetCore.Antiforgery.IAntiforgery antiforgery, HttpContext context) =>
    {
        var tokens = antiforgery.GetAndStoreTokens(context);
        return Results.Ok(new { requestToken = tokens.RequestToken });
    }).AllowAnonymous();

    app.UseAuthorization();

    app.MapControllers();
    app.MapLocalEatsClientFallback();

    app.Run();
}
catch(Exception ex)
{
    Log.Fatal(ex, "Application startup failed");
}
finally
{
    Log.CloseAndFlush();
}


public partial class Program { }
