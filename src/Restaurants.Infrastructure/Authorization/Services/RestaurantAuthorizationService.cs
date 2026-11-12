using Microsoft.Extensions.Logging;
using Restaurants.Application.User;
using Restaurants.Domain.Constants;
using Restaurants.Domain.Entities;
using Restaurants.Domain.Interfaces;

namespace Restaurants.Infrastructure.Authorization.Services;

public class RestaurantAuthorizationService(
    ILogger<RestaurantAuthorizationService> logger,
    IUserContext userContext) : IRestaurantAuthorizationService
{
    public bool Authorize(Restaurant restaurant, ResourceOperation resourceOperation)
    {
        if (resourceOperation == ResourceOperation.Read)
        {
            // Restaurant catalog reads are intentionally public.
            return true;
        }

        if (resourceOperation != ResourceOperation.Update)
        {
            // Creation is authorized by the endpoint role and the create handler's
            // authenticated-user check; this resource-level check has no owner yet.
            logger.LogWarning("Denying unsupported {Operation} authorization for restaurant {RestaurantId}", resourceOperation, restaurant.Id);
            return false;
        }

        var user = userContext.GetCurrentUser();
        if (user is null)
        {
            logger.LogWarning("Denying {Operation} on restaurant {RestaurantId}: no authenticated user", resourceOperation, restaurant.Id);
            return false;
        }

        var isOwner = string.Equals(user.Id, restaurant.OwnerId, StringComparison.Ordinal);
        if (!isOwner)
        {
            logger.LogWarning("Denying {Operation} on restaurant {RestaurantId} for user {UserId}: user is not the owner",
                resourceOperation, restaurant.Id, user.Id);
            return false;
        }

        logger.LogInformation("Authorized owner {UserId} for {Operation} on restaurant {RestaurantId}",
            user.Id, resourceOperation, restaurant.Id);
        return true;
    }
}
