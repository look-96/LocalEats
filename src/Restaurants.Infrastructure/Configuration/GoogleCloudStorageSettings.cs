namespace Restaurants.Infrastructure.Configuration;

public sealed class GoogleCloudStorageSettings
{
    public string Bucket { get; set; } = string.Empty;
    public string LogoPrefix { get; set; } = "restaurant-logos";
    public int SignedUrlLifetimeMinutes { get; set; } = 30;
}
