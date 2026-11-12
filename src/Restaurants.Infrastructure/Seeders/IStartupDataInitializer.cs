namespace Restaurants.Infrastructure.Seeders;

public interface IStartupDataInitializer
{
    Task InitializeAsync(CancellationToken cancellationToken = default);
}
