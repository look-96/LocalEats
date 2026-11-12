
using Restaurants.Domain.Exceptions;

namespace Restaurants.API.Middlewares;

public class ErrorHandlingMiddleware(ILogger<ErrorHandlingMiddleware> logger) : IMiddleware
{
    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        try
        {
            await next.Invoke(context);
        }
        catch(NotFoundException)
        {
            context.Response.StatusCode = 404;
            await context.Response.WriteAsync("Resource not found.");

            logger.LogWarning("API resource lookup returned not found.");
        }
        catch(ForbidException)
        {
            context.Response.StatusCode = 403;
            await context.Response.WriteAsync("Access forbidden");
        }
        catch(Exception ex)
        {
            logger.LogError(ex, "Unhandled API exception.");

            context.Response.StatusCode = 500;
            await context.Response.WriteAsync("Something went wrong");
        }
    }
}
