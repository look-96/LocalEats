using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Restaurants.Infrastructure.Persistance;

namespace Restaurants.API.Persistence;

/// <summary>
/// Provides PostgreSQL options to EF tooling without booting the API or connecting to a database.
/// </summary>
public sealed class DesignTimeRestaurantsDbContextFactory : IDesignTimeDbContextFactory<RestaurantsDbContext>
{
    public RestaurantsDbContext CreateDbContext(string[] args)
    {
        var connectionString = Environment.GetEnvironmentVariable("LOCALEATS_DESIGNTIME_CONNECTION_STRING")
            ?? "Host=localhost;Database=localeats_dev;Username=design_time";

        var options = new DbContextOptionsBuilder<RestaurantsDbContext>()
            .UseNpgsql(connectionString, postgres =>
                postgres.MigrationsAssembly("Restaurants.PostgreSqlMigrations"))
            .Options;

        return new RestaurantsDbContext(options);
    }
}
