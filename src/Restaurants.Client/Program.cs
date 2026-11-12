using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Restaurants.Client;
using Restaurants.Client.Auth;
using Restaurants.Client.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

var configuredApiBaseUrl = builder.Configuration["ApiBaseUrl"];
var apiBaseUri = string.IsNullOrWhiteSpace(configuredApiBaseUrl)
    ? new Uri(builder.HostEnvironment.BaseAddress)
    : new Uri(new Uri(builder.HostEnvironment.BaseAddress), configuredApiBaseUrl);

if (apiBaseUri.Scheme != Uri.UriSchemeHttps &&
    !(builder.HostEnvironment.Environment == "Development" && apiBaseUri.IsLoopback))
{
    throw new InvalidOperationException("ApiBaseUrl must resolve to HTTPS outside local development.");
}

builder.Services.AddScoped<AntiforgeryTokenProvider>();
builder.Services.AddTransient<ApiSecurityHandler>();
builder.Services.AddHttpClient("RestaurantsApi", client => client.BaseAddress = apiBaseUri)
    .AddHttpMessageHandler<ApiSecurityHandler>();
builder.Services.AddHttpClient("IdentityApi", client => client.BaseAddress = apiBaseUri)
    .AddHttpMessageHandler<ApiSecurityHandler>();
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<AuthenticationStateProvider>(services => services.GetRequiredService<AuthService>());
builder.Services.AddAuthorizationCore();
builder.Services.AddScoped(services => services.GetRequiredService<IHttpClientFactory>().CreateClient("RestaurantsApi"));
builder.Services.AddScoped<RestaurantService>();
builder.Services.AddScoped<DishService>();

await builder.Build().RunAsync();
