using System.Net;
using System.Net.Http.Json;
using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;
using Restaurants.Client.Models;
using Restaurants.Client.Models.Auth;

namespace Restaurants.Client.Services;

public sealed class AuthService(IHttpClientFactory httpClientFactory) : AuthenticationStateProvider
{
    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        try
        {
            using var response = await httpClientFactory.CreateClient("IdentityApi")
                .GetAsync("api/identity/session");
            if (response.StatusCode == HttpStatusCode.Unauthorized)
                return AnonymousState();

            response.EnsureSuccessStatusCode();
            var session = await response.Content.ReadFromJsonAsync<AuthenticatedSession>();
            return session is null ? AnonymousState() : CreateState(session);
        }
        catch (HttpRequestException)
        {
            return AnonymousState();
        }
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
            .PostAsJsonAsync("api/identity/login?useCookies=true&useSessionCookies=true", request, cancellationToken);
        if (!response.IsSuccessStatusCode)
            throw new InvalidOperationException(await ApiErrorReader.ReadAsync(response, cancellationToken));

        var state = await GetAuthenticationStateAsync();
        if (state.User.Identity?.IsAuthenticated != true)
            throw new InvalidOperationException("Sign-in succeeded, but the session could not be confirmed.");

        NotifyAuthenticationStateChanged(Task.FromResult(state));
    }

    public async Task LogoutAsync(CancellationToken cancellationToken = default)
    {
        using var response = await httpClientFactory.CreateClient("IdentityApi")
            .PostAsync("api/identity/logout", content: null, cancellationToken);
        if (!response.IsSuccessStatusCode && response.StatusCode != HttpStatusCode.Unauthorized)
            throw new InvalidOperationException(await ApiErrorReader.ReadAsync(response, cancellationToken));

        NotifyAuthenticationStateChanged(Task.FromResult(AnonymousState()));
    }

    private static AuthenticationState CreateState(AuthenticatedSession session)
    {
        var claims = new List<Claim>();
        if (!string.IsNullOrWhiteSpace(session.UserId))
            claims.Add(new Claim(ClaimTypes.NameIdentifier, session.UserId));
        if (!string.IsNullOrWhiteSpace(session.Email))
        {
            claims.Add(new Claim(ClaimTypes.Email, session.Email));
            claims.Add(new Claim(ClaimTypes.Name, session.Email));
        }
        claims.AddRange(session.Roles.Select(role => new Claim(ClaimTypes.Role, role)));

        var identity = new ClaimsIdentity(claims, "RestaurantsApiCookie", ClaimTypes.Name, ClaimTypes.Role);
        return new AuthenticationState(new ClaimsPrincipal(identity));
    }

    private static AuthenticationState AnonymousState() =>
        new(new ClaimsPrincipal(new ClaimsIdentity()));
}
