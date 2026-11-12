using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Restaurants.Application.User;

namespace Restaurants.API.Services;

public sealed class HttpUserContext(IHttpContextAccessor httpContextAccessor) : IUserContext
{
    public CurrentUser? GetCurrentUser()
    {
        var principal = httpContextAccessor.HttpContext?.User;
        if (principal?.Identity?.IsAuthenticated != true)
        {
            return null;
        }

        var userId = principal.FindFirstValue(ClaimTypes.NameIdentifier);
        var email = principal.FindFirstValue(ClaimTypes.Email);
        if (string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(email))
        {
            return null;
        }

        var roles = principal.FindAll(ClaimTypes.Role).Select(claim => claim.Value);
        var nationality = principal.FindFirstValue("Nationality");

        return new CurrentUser(userId, email, roles, nationality);
    }
}
