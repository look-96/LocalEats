using Microsoft.AspNetCore.StaticFiles;

namespace Restaurants.API.Extensions;

public static class ClientHostingExtensions
{
    public static void UseLocalEatsClientFiles(this WebApplication app)
    {
        app.UseDefaultFiles();
        var contentTypes = new FileExtensionContentTypeProvider();
        contentTypes.Mappings[".dat"] = "application/octet-stream";
        contentTypes.Mappings[".pdb"] = "application/octet-stream";
        app.UseStaticFiles(new StaticFileOptions { ContentTypeProvider = contentTypes });
    }

    public static void MapLocalEatsClientFallback(this WebApplication app)
    {
        app.MapFallback("{*path:nonfile}", async context =>
        {
            var path = context.Request.Path;
            if (path.StartsWithSegments("/api") || path.StartsWithSegments("/health") ||
                path.StartsWithSegments("/swagger"))
            {
                context.Response.StatusCode = StatusCodes.Status404NotFound;
                return;
            }

            var webRoot = app.Environment.WebRootPath ?? Path.Combine(AppContext.BaseDirectory, "wwwroot");
            var indexPath = Path.Combine(webRoot, "index.html");
            if (!File.Exists(indexPath))
            {
                context.Response.StatusCode = StatusCodes.Status404NotFound;
                return;
            }

            context.Response.ContentType = "text/html; charset=utf-8";
            context.Response.Headers.CacheControl = "no-cache, no-store, must-revalidate";
            await context.Response.SendFileAsync(indexPath);
        });
    }
}
