using System.Net.Http.Json;
using Restaurants.Client.Models;

namespace Restaurants.Client.Services;

public sealed class DishService(HttpClient httpClient)
{
    public async Task<IReadOnlyList<Dish>> GetForRestaurantAsync(int restaurantId, CancellationToken cancellationToken = default)
    {
        using var response = await httpClient.GetAsync($"api/restaurants/{restaurantId}/dishes", cancellationToken);
        if (!response.IsSuccessStatusCode)
            throw new InvalidOperationException(await ApiErrorReader.ReadAsync(response, cancellationToken));

        return await response.Content.ReadFromJsonAsync<List<Dish>>(cancellationToken: cancellationToken)
            ?? throw new InvalidOperationException("The API returned an empty dish list.");
    }

    public async Task CreateAsync(int restaurantId, DishCreateRequest request, CancellationToken cancellationToken = default)
    {
        ClientInputSanitizer.Normalize(request);
        using var response = await httpClient.PostAsJsonAsync($"api/restaurants/{restaurantId}/dishes", request, cancellationToken);
        if (!response.IsSuccessStatusCode)
            throw new InvalidOperationException(await ApiErrorReader.ReadAsync(response, cancellationToken));
    }

    public async Task UpdateAsync(int restaurantId, int dishId, DishCreateRequest request, CancellationToken cancellationToken = default)
    {
        ClientInputSanitizer.Normalize(request);
        using var response = await httpClient.PutAsJsonAsync(
            $"api/restaurants/{restaurantId}/dishes/{dishId}", request, cancellationToken);
        if (!response.IsSuccessStatusCode)
            throw new InvalidOperationException(await ApiErrorReader.ReadAsync(response, cancellationToken));
    }
}
