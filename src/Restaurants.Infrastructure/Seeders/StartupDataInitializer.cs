using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace Restaurants.Infrastructure.Seeders;

internal sealed class StartupDataInitializer(
    DatabaseMigrator databaseMigrator,
    SampleRestaurantSeeder sampleRestaurantSeeder,
    IdentityRoleSeeder identityRoleSeeder,
    BootstrapAdminProvisioner bootstrapAdminProvisioner,
    IConfiguration configuration,
    IHostEnvironment environment,
    ILogger<StartupDataInitializer> logger) : IStartupDataInitializer
{
    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        var applyMigrationsAtStartup = environment.IsDevelopment() &&
            configuration.GetValue("Database:ApplyMigrationsAtStartup", true);
        if (applyMigrationsAtStartup)
        {
            await databaseMigrator.MigrateAsync(cancellationToken);
        }

        if (!await databaseMigrator.CanConnectAsync(cancellationToken))
        {
            logger.LogWarning("Database is unavailable; startup seed data and bootstrap Admin provisioning were skipped.");
            return;
        }

        if (environment.IsDevelopment())
        {
            await sampleRestaurantSeeder.SeedAsync(cancellationToken);
        }
        await identityRoleSeeder.SeedAsync(cancellationToken);
        await bootstrapAdminProvisioner.ProvisionAsync(cancellationToken);
    }
}
