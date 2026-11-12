namespace Restaurants.Domain.Interfaces;

public interface IRestaurantLogoStorage
{
    Task<string> UploadAsync(Stream data, string fileName, string contentType, CancellationToken cancellationToken = default);
    Task<string?> CreateReadUrlAsync(string? storageKey, CancellationToken cancellationToken = default);
}
