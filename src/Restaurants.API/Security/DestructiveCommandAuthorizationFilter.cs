using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Restaurants.API.Security;
using Restaurants.Domain.Constants;

namespace Restaurants.API.Security;

internal sealed class DestructiveCommandAuthorizationFilter(
    IAdminCredentialVerifier credentialVerifier,
    ILogger<DestructiveCommandAuthorizationFilter> logger) : IAsyncActionFilter
{
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var isAdmin = context.HttpContext.User.Identity?.IsAuthenticated == true &&
            context.HttpContext.User.IsInRole(UserRoles.Admin);
        var credential = context.HttpContext.Request.Headers["X-LocalEats-Admin-Key"].ToString();

        if (!isAdmin || !credentialVerifier.IsValid(credential))
        {
            logger.LogWarning(
                "Blocked destructive HTTP request. ActorId: {ActorId}; IsAdmin: {IsAdmin}",
                context.HttpContext.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value,
                isAdmin);
            context.Result = new ForbidResult();
            return;
        }

        await next();
    }
}
