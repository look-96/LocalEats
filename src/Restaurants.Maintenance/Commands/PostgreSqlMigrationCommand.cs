using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Restaurants.Infrastructure.Persistance;

namespace Restaurants.Maintenance.Commands;

internal sealed class PostgreSqlMigrationCommand(
    RestaurantsDbContext dbContext,
    ILogger<PostgreSqlMigrationCommand> logger)
{
    public async Task RunAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("Checking for pending LocalEats PostgreSQL migrations.");
        var pendingMigrations = await dbContext.Database.GetPendingMigrationsAsync(cancellationToken);
        if (!pendingMigrations.Any())
        {
            logger.LogInformation("No pending LocalEats PostgreSQL migrations.");
            return;
        }

        await dbContext.Database.MigrateAsync(cancellationToken);
        logger.LogInformation("LocalEats PostgreSQL migrations were applied successfully.");
    }
}
