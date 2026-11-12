using MediatR;
using Microsoft.Extensions.Logging;
using Restaurants.Domain.Constants;
using Restaurants.Domain.Entities;
using Restaurants.Domain.Exceptions;
using Restaurants.Domain.Interfaces;
using Restaurants.Domain.Repositories;

namespace Restaurants.Application.Dishes.Commands.UpdateDish;

public sealed class UpdateDishCommandHandler(
    ILogger<UpdateDishCommandHandler> logger,
    IRestaurantsRepository restaurantsRepository,
    IRestaurantAuthorizationService restaurantAuthorizationService)
    : IRequestHandler<UpdateDishCommand>
{
    public async Task Handle(UpdateDishCommand request, CancellationToken cancellationToken)
    {
        var restaurant = await restaurantsRepository.GetByIdAsync(request.RestaurantId);
        if (restaurant is null)
            throw new NotFoundException(nameof(Restaurant), request.RestaurantId.ToString());

        if (!restaurantAuthorizationService.Authorize(restaurant, ResourceOperation.Update))
            throw new ForbidException();

        var dish = restaurant.Dishes.FirstOrDefault(item => item.Id == request.DishId);
        if (dish is null)
            throw new NotFoundException(nameof(Dish), request.DishId.ToString());

        dish.Name = request.Name;
        dish.Description = request.Description;
        dish.Price = request.Price;
        dish.KiloCalories = request.KiloCalories;

        await restaurantsRepository.SaveChanges();
        logger.LogInformation("Updated dish {DishId} for restaurant {RestaurantId}", dish.Id, restaurant.Id);
    }
}
