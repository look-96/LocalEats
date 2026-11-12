
using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using Restaurants.Application.User;
using Restaurants.Domain.Entities;
using Restaurants.Domain.Repositories;

namespace Restaurants.Application.Restaurants.Commands.CreateRestaurant;

public class CreateRestaurantCommandHandler(ILogger<CreateRestaurantCommandHandler> logger,
    IMapper mapper,
    IRestaurantsRepository restaurantsRepository,
    IUserContext userContext) : IRequestHandler<CreateRestaurantCommand, int>
{
    public async Task<int> Handle(CreateRestaurantCommand request, CancellationToken cancellationToken)
    {
        var currentUser = userContext.GetCurrentUser();
        if (currentUser is null)
        {
            throw new UnauthorizedAccessException("An authenticated user is required to create a restaurant.");
        }

        logger.LogInformation("User {UserId} is creating a restaurant", currentUser.Id);

        var restaurant = mapper.Map<Restaurant>(request);
        restaurant.OwnerId = currentUser.Id;    

        int id = await restaurantsRepository.Create(restaurant);
        logger.LogInformation("Created restaurant {RestaurantId} for user {UserId}", id, currentUser.Id);
        return id;
    }
}
