using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.OutputCaching;
using Restaurants.Application.Dishes.Commands.CreateDish;
using Restaurants.Application.Dishes.Commands.UpdateDish;
using Restaurants.Application.Dishes.Dtos;
using Restaurants.Application.Dishes.Queries.GetDishByIdForRestaurant;
using Restaurants.Application.Dishes.Queries.GetDishesForRestaurant;
using Restaurants.Domain.Entities;

namespace Restaurants.API.Controllers;

[Route("api/restaurants/{restaurantId}/dishes")]
[ApiController]
public class DishesController(IMediator mediator, IOutputCacheStore outputCacheStore) : ControllerBase
{
    [HttpPost]
    [Authorize]
    public async Task<IActionResult> CreateDish([FromRoute]int restaurantId, CreateDishCommand command)
    {
        command.RestaurantId = restaurantId;

        var dishId = await mediator.Send(command);
        await outputCacheStore.EvictByTagAsync("public-catalog", CancellationToken.None);
        return CreatedAtAction(nameof(GetByIdForRestaurant), new { restaurantId, dishId }, null);
    }

    [HttpPut("{dishId:int}")]
    [Authorize]
    public async Task<IActionResult> UpdateDish(
        [FromRoute] int restaurantId,
        [FromRoute] int dishId,
        UpdateDishCommand command)
    {
        command.RestaurantId = restaurantId;
        command.DishId = dishId;

        await mediator.Send(command);
        await outputCacheStore.EvictByTagAsync("public-catalog", CancellationToken.None);
        return NoContent();
    }

    [HttpGet]
    [AllowAnonymous]
    [OutputCache(PolicyName = "PublicCatalog")]
    public async Task<ActionResult<IEnumerable<DishDto>>> GetAllForRestaurant([FromRoute] int restaurantId)
    {
        var dishes = await mediator.Send(new GetDishesForRestaurantQuery(restaurantId));
        return Ok(dishes);
    }

    [HttpGet("{dishId}")]
    [AllowAnonymous]
    [OutputCache(PolicyName = "PublicCatalog")]
    public async Task<ActionResult<DishDto>> GetByIdForRestaurant([FromRoute] int restaurantId, [FromRoute] int dishId)
    {
        var dish = await mediator.Send(new GetDishByIdForRestaurantQuery(restaurantId, dishId));
        return Ok(dish);
    }

}
