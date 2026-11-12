using AutoMapper;
using MediatR;
using Restaurants.Application.Restaurants.Dtos;
using Restaurants.Application.User;
using Restaurants.Domain.Repositories;

namespace Restaurants.Application.Restaurants.Queries.GetMyRestaurants;

public sealed class GetMyRestaurantsQueryHandler(
    IUserContext userContext,
    IRestaurantsRepository restaurantsRepository,
    IMapper mapper) : IRequestHandler<GetMyRestaurantsQuery, IEnumerable<RestaurantDto>>
{
    public async Task<IEnumerable<RestaurantDto>> Handle(
        GetMyRestaurantsQuery request,
        CancellationToken cancellationToken)
    {
        var currentUser = userContext.GetCurrentUser()
            ?? throw new UnauthorizedAccessException("Sign in to view your restaurant listings.");

        var restaurants = await restaurantsRepository.GetOwnedByAsync(currentUser.Id);
        return mapper.Map<IEnumerable<RestaurantDto>>(restaurants);
    }
}
