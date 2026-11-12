using Microsoft.JSInterop;
using Restaurants.Client.Models.Auth;

namespace Restaurants.Client.Auth;

public sealed class BrowserTokenStore(IJSRuntime jsRuntime)
{
    public ValueTask<TokenSession?> GetAsync() =>
        jsRuntime.InvokeAsync<TokenSession?>("restaurantsAuth.getSession");

    public ValueTask SetAsync(TokenSession session) =>
        jsRuntime.InvokeVoidAsync("restaurantsAuth.setSession", session);

    public ValueTask ClearAsync() =>
        jsRuntime.InvokeVoidAsync("restaurantsAuth.clearSession");
}
