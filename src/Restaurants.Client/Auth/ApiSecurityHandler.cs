using Microsoft.AspNetCore.Components.WebAssembly.Http;

namespace Restaurants.Client.Auth;

public sealed class ApiSecurityHandler(AntiforgeryTokenProvider antiforgeryTokenProvider) : DelegatingHandler
{
    private static readonly HashSet<string> SafeMethods = ["GET", "HEAD", "OPTIONS", "TRACE"];

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        request.SetBrowserRequestCredentials(BrowserRequestCredentials.Include);

        if (!SafeMethods.Contains(request.Method.Method))
        {
            var token = await antiforgeryTokenProvider.GetRequestTokenAsync(cancellationToken);
            request.Headers.TryAddWithoutValidation("X-CSRF-TOKEN", token);
        }

        return await base.SendAsync(request, cancellationToken);
    }
}
