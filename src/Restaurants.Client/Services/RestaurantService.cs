using System.Net.Http.Json;
using Restaurants.Client.Models;

namespace Restaurants.Client.Services;

public sealed class RestaurantService(HttpClient httpClient)
{
    public async Task<PagedResult<Restaurant>> GetPageAsync(
        string? searchPhrase,
        int pageNumber,
        int pageSize,
        string? sortBy,
        string sortDirection,
        CancellationToken cancellationToken = default)
    {
        searchPhrase = ClientInputSanitizer.NormalizeText(searchPhrase);
        var query = new List<string>
        {
            $"PageNumber={pageNumber}",
            $"PageSize={pageSize}",
            $"SortDirection={Uri.EscapeDataString(sortDirection)}"
        };
        if (!string.IsNullOrWhiteSpace(searchPhrase))
            query.Add($"SearchPhrase={Uri.EscapeDataString(searchPhrase.Trim())}");
        if (!string.IsNullOrWhiteSpace(sortBy))
            query.Add($"SortBy={Uri.EscapeDataString(sortBy)}");

        using var response = await httpClient.GetAsync($"api/restaurants?{string.Join('&', query)}", cancellationToken);
        if (!response.IsSuccessStatusCode)
            throw new InvalidOperationException(await ApiErrorReader.ReadAsync(response, cancellationToken));

        return await response.Content.ReadFromJsonAsync<PagedResult<Restaurant>>(cancellationToken: cancellationToken)
            ?? throw new InvalidOperationException("The API returned an empty restaurant page.");
    }

    public async Task<Restaurant> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        using var response = await httpClient.GetAsync($"api/restaurants/{id}", cancellationToken);
        if (!response.IsSuccessStatusCode)
            throw new InvalidOperationException(await ApiErrorReader.ReadAsync(response, cancellationToken));

        return await response.Content.ReadFromJsonAsync<Restaurant>(cancellationToken: cancellationToken)
            ?? throw new InvalidOperationException("The API returned an empty restaurant response.");
    }

    public async Task<IReadOnlyList<Restaurant>> GetMineAsync(CancellationToken cancellationToken = default)
    {
        using var response = await httpClient.GetAsync("api/restaurants/mine", cancellationToken);
        if (!response.IsSuccessStatusCode)
            throw new InvalidOperationException(await ApiErrorReader.ReadAsync(response, cancellationToken));

        return await response.Content.ReadFromJsonAsync<List<Restaurant>>(cancellationToken: cancellationToken)
            ?? throw new InvalidOperationException("The API returned an empty restaurant list.");
    }

    public async Task<int> CreateAsync(RestaurantCreateRequest request, CancellationToken cancellationToken = default)
    {
        ClientInputSanitizer.Normalize(request);
        using var response = await httpClient.PostAsJsonAsync("api/restaurants", request, cancellationToken);
        if (!response.IsSuccessStatusCode)
            throw new InvalidOperationException(await ApiErrorReader.ReadAsync(response, cancellationToken));

        var location = response.Headers.Location?.ToString().TrimEnd('/');
        if (int.TryParse(location?.Split('/').LastOrDefault(), out var id))
            return id;

        throw new InvalidOperationException("The API created the restaurant but did not return its location.");
    }

    public async Task UpdateAsync(int id, RestaurantUpdateRequest request, CancellationToken cancellationToken = default)
    {
        ClientInputSanitizer.Normalize(request);
        using var response = await httpClient.PatchAsJsonAsync($"api/restaurants/{id}", request, cancellationToken);
        if (!response.IsSuccessStatusCode)
            throw new InvalidOperationException(await ApiErrorReader.ReadAsync(response, cancellationToken));
    }

    public async Task UploadLogoAsync(int id, Stream fileStream, string fileName, string contentType, CancellationToken cancellationToken = default)
    {
        using var content = new MultipartFormDataContent();
        using var fileContent = new StreamContent(fileStream);
        fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(contentType);
        content.Add(fileContent, "file", fileName);
        using var response = await httpClient.PostAsync($"api/restaurants/{id}/logo", content, cancellationToken);
        if (!response.IsSuccessStatusCode)
            throw new InvalidOperationException(await ApiErrorReader.ReadAsync(response, cancellationToken));
    }
}
