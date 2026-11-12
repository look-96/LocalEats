using Microsoft.EntityFrameworkCore;
using Restaurants.Infrastructure.Persistance;

namespace Restaurants.Infrastructure.Seeders;

internal sealed class DatabaseMigrator(RestaurantsDbContext dbContext)
{
    public async Task MigrateAsync(CancellationToken cancellationToken)
    {
        var pendingMigrations = await dbContext.Database.GetPendingMigrationsAsync(cancellationToken);
        if (pendingMigrations.Any())
        {
            await dbContext.Database.MigrateAsync(cancellationToken);
        }
    }

    public Task<bool> CanConnectAsync(CancellationToken cancellationToken) =>
        dbContext.Database.CanConnectAsync(cancellationToken);
}
