using System.Net.Http.Json;
using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;
using Restaurants.Client.Auth;
using Restaurants.Client.Models;
using Restaurants.Client.Models.Auth;

namespace Restaurants.Client.Services;

public sealed class AuthService(
    IHttpClientFactory httpClientFactory,
    BrowserTokenStore tokenStore) : AuthenticationStateProvider
{
    private readonly SemaphoreSlim refreshLock = new(1, 1);

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        var session = await tokenStore.GetAsync();
        if (session is null || string.IsNullOrWhiteSpace(session.AccessToken))
            return AnonymousState();

        var identity = new ClaimsIdentity([new Claim(ClaimTypes.Name, "Signed-in user")], "RestaurantsApiToken");
        return new AuthenticationState(new ClaimsPrincipal(identity));
    }

    public async Task RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default)
    {
        ClientInputSanitizer.Normalize(request);
        using var response = await httpClientFactory.CreateClient("IdentityApi")
            .PostAsJsonAsync("api/identity/register", request, cancellationToken);
        if (!response.IsSuccessStatusCode)
            throw new InvalidOperationException(await ApiErrorReader.ReadAsync(response, cancellationToken));
    }

    public async Task LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
    {
        ClientInputSanitizer.Normalize(request);
        using var response = await httpClientFactory.CreateClient("IdentityApi")
            .PostAsJsonAsync("api/identity/login?useCookies=false", request, cancellationToken);
        if (!response.IsSuccessStatusCode)
            throw new InvalidOperationException(await ApiErrorReader.ReadAsync(response, cancellationToken));

        var tokenResponse = await response.Content.ReadFromJsonAsync<AccessTokenResponse>(cancellationToken: cancellationToken)
            ?? throw new InvalidOperationException("The sign-in response was empty.");
        await SaveSessionAsync(tokenResponse);
        NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
    }

    public async Task<string?> GetValidAccessTokenAsync(CancellationToken cancellationToken = default)
    {
        var session = await tokenStore.GetAsync();
        if (session is null || string.IsNullOrWhiteSpace(session.AccessToken))
            return null;

        var expiry = DateTimeOffset.FromUnixTimeMilliseconds(session.ExpiresAtUnixMilliseconds);
        if (expiry - DateTimeOffset.UtcNow > TimeSpan.FromSeconds(30))
            return session.AccessToken;

        await refreshLock.WaitAsync(cancellationToken);
        try
        {
            session = await tokenStore.GetAsync();
            if (session is null || string.IsNullOrWhiteSpace(session.RefreshToken))
            {
                await LogoutAsync();
                return null;
            }

            expiry = DateTimeOffset.FromUnixTimeMilliseconds(session.ExpiresAtUnixMilliseconds);
            if (expiry - DateTimeOffset.UtcNow > TimeSpan.FromSeconds(30))
                return session.AccessToken;

            using var response = await httpClientFactory.CreateClient("IdentityApi")
                .PostAsJsonAsync("api/identity/refresh", new RefreshTokenRequest(session.RefreshToken), cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                await LogoutAsync();
                return null;
            }

            var tokenResponse = await response.Content.ReadFromJsonAsync<AccessTokenResponse>(cancellationToken: cancellationToken);
            if (tokenResponse is null)
            {
                await LogoutAsync();
                return null;
            }

            await SaveSessionAsync(tokenResponse);
            return tokenResponse.AccessToken;
        }
        finally
        {
            refreshLock.Release();
        }
    }

    public async Task LogoutAsync()
    {
        await tokenStore.ClearAsync();
        NotifyAuthenticationStateChanged(Task.FromResult(AnonymousState()));
    }

    private async Task SaveSessionAsync(AccessTokenResponse tokenResponse)
    {
        var session = new TokenSession
        {
            AccessToken = tokenResponse.AccessToken,
            RefreshToken = tokenResponse.RefreshToken,
            ExpiresAtUnixMilliseconds = DateTimeOffset.UtcNow.AddSeconds(tokenResponse.ExpiresIn).ToUnixTimeMilliseconds()
        };
        await tokenStore.SetAsync(session);
    }

    private static AuthenticationState AnonymousState() =>
        new(new ClaimsPrincipal(new ClaimsIdentity()));
}
