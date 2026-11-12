using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;
using Restaurants.Application.Restaurants.Commands.CreateRestaurant;
using Restaurants.Application.Restaurants.Commands.UpdateRestaurant;
using Restaurants.Application.Restaurants.Commands.UploadRestaurantLogo;
using Restaurants.Application.Restaurants.Dtos;
using Restaurants.Application.Restaurants.Queries.GetAllRestaurants;
using Restaurants.Application.Restaurants.Queries.GetRestaurantById;
using Restaurants.Application.Restaurants.Queries.GetMyRestaurants;
using Restaurants.Domain.Constants;
using Restaurants.Infrastructure.Authorization;
using Restaurants.API.Validation;

namespace Restaurants.API.Controllers;

[ApiController]
[Route("api/restaurants")]
[Authorize]
public class RestaurantsController(IMediator mediator, IOutputCacheStore outputCacheStore) : ControllerBase
{
    [HttpGet]
    [AllowAnonymous]
    [OutputCache(PolicyName = "PublicCatalog")]
    //[Authorize(Policy = PolicyNames.CreatedAtLeast2Restaurants)]
    public async Task<ActionResult<IEnumerable<RestaurantDto>>> GetAll([FromQuery]GetAllRestaurantsQuery query)
    {
        var restaurants = await mediator.Send(query);
            return Ok(restaurants);
    }

    [HttpGet("mine")]
    [Authorize(Roles = UserRoles.Owner)]
    public async Task<ActionResult<IEnumerable<RestaurantDto>>> GetMine()
    {
        var restaurants = await mediator.Send(new GetMyRestaurantsQuery());
        return Ok(restaurants);
    }

    [HttpGet("{id:int}")]
    [AllowAnonymous]
    [OutputCache(PolicyName = "PublicCatalog")]
    //[Authorize(Policy = PolicyNames.HasNationality)]
    public async Task<ActionResult<RestaurantDto?>> GetById([FromRoute]int id)
    {
        var restaurant = await mediator.Send(new GetRestaurantByIdQuery(id));

        return Ok(restaurant);
    }

    [HttpPatch("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateRestaurant([FromRoute] int id, UpdateRestaurantCommand command)
    {
        command.Id = id;
        await mediator.Send(command);
        await EvictPublicCatalogAsync();

        return NoContent();
    }


    [HttpPost]
    [Authorize(Roles = UserRoles.Owner)]
    public async Task<IActionResult> CreateRestaurant(CreateRestaurantCommand command)
    {
        int id = await mediator.Send(command);
        await EvictPublicCatalogAsync();
        return CreatedAtAction(nameof(GetById), new { id }, null);
    }
    
    [HttpPost("{id}/logo")]
    [RequestSizeLimit(ImageFileValidator.MaxRequestSize)]
    public async Task<IActionResult> UploadLogo([FromRoute]int id, IFormFile file, CancellationToken cancellationToken)
    {
        var contentType = await ImageFileValidator.GetValidatedContentTypeAsync(file, cancellationToken);
        if (contentType is null)
            return BadRequest("Upload a valid PNG, JPEG or WebP image up to 5 MB.");

        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        using var stream = file.OpenReadStream();

        var command = new UploadRestaurantLogoCommand()
        {
            RestaurantId = id,
            FileName = $"{id}-{Guid.NewGuid():N}{extension}",
            ContentType = contentType,
            File = stream
        };

        await mediator.Send(command);
        await EvictPublicCatalogAsync();
        return NoContent();
    }

    private ValueTask EvictPublicCatalogAsync() =>
        outputCacheStore.EvictByTagAsync("public-catalog", CancellationToken.None);

}
