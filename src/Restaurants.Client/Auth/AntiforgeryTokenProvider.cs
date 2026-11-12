using System.Net.Http.Json;
using Restaurants.Client.Models.Auth;

namespace Restaurants.Client.Auth;

public sealed class AntiforgeryTokenProvider(IHttpClientFactory httpClientFactory)
{
    public async Task<string> GetRequestTokenAsync(CancellationToken cancellationToken = default)
    {
        using var response = await httpClientFactory.CreateClient("RestaurantsApi")
            .GetAsync("api/security/antiforgery", cancellationToken);
        response.EnsureSuccessStatusCode();

        var tokens = await response.Content.ReadFromJsonAsync<AntiforgeryTokenResponse>(cancellationToken: cancellationToken);
        return !string.IsNullOrWhiteSpace(tokens?.RequestToken)
            ? tokens.RequestToken
            : throw new InvalidOperationException("The API did not provide an antiforgery token.");
    }
}
