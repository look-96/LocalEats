using MediatR;
using Restaurants.Application.Restaurants.Dtos;

namespace Restaurants.Application.Restaurants.Queries.GetMyRestaurants;

public sealed record GetMyRestaurantsQuery : IRequest<IEnumerable<RestaurantDto>>;
