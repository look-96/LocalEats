using Restaurants.Domain.Constants;
using Restaurants.Domain.Entities;

namespace Restaurants.Domain.Repositories;

public interface IRestaurantsRepository
{
    Task<IEnumerable<Restaurant>> GetAllAsync();
    Task<IEnumerable<Restaurant>> GetOwnedByAsync(string ownerId);
    Task<Restaurant?> GetByIdAsync(int id);
    Task<int> Create(Restaurant entity);
    Task SaveChanges();
    Task<(IEnumerable<Restaurant>, int)> GetAllMatchingAsync(string? searchPhrase, int pageSize, int pageNumber, string? sortBy, SortDirection sortDirection);
}
