using Microsoft.AspNetCore.Antiforgery;

namespace Restaurants.API.Security;

internal sealed class AntiforgeryValidationMiddleware(
    RequestDelegate next,
    ILogger<AntiforgeryValidationMiddleware> logger)
{
    private const string AuthenticationCookieName = "__Host-LocalEats.Auth";
    private static readonly HashSet<string> SafeMethods = ["GET", "HEAD", "OPTIONS", "TRACE"];

    public async Task InvokeAsync(HttpContext context, IAntiforgery antiforgery)
    {
        var isApiRequest = context.Request.Path.StartsWithSegments("/api");
        var hasAuthenticationCookie = context.Request.Cookies.ContainsKey(AuthenticationCookieName);
        var isUnsafeMethod = !SafeMethods.Contains(context.Request.Method);

        if (isApiRequest && hasAuthenticationCookie && isUnsafeMethod)
        {
            try
            {
                await antiforgery.ValidateRequestAsync(context);
            }
            catch (AntiforgeryValidationException exception)
            {
                logger.LogWarning(
                    "Antiforgery validation rejected {Method} {Path}. HeaderPresent={HeaderPresent}, CookiePresent={CookiePresent}. Reason: {Reason}",
                    context.Request.Method,
                    context.Request.Path,
                    context.Request.Headers.ContainsKey("X-CSRF-TOKEN"),
                    context.Request.Cookies.ContainsKey("__Host-LocalEats.Antiforgery"),
                    exception.Message);

                context.Response.StatusCode = StatusCodes.Status400BadRequest;
                await context.Response.WriteAsJsonAsync(new
                {
                    title = "Antiforgery validation failed",
                    status = StatusCodes.Status400BadRequest,
                    detail = "Refresh the page and try the request again."
                });
                return;
            }
        }

        await next(context);
    }
}
