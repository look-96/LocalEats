using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Restaurants.Domain.Entities;
using Restaurants.Domain.Interfaces;
using Restaurants.Domain.Repositories;
using Restaurants.Infrastructure.Authorization;
using Restaurants.Infrastructure.Authorization.Requirements;
using Restaurants.Infrastructure.Authorization.Services;
using Restaurants.Infrastructure.Configuration;
using Restaurants.Infrastructure.GoogleCloud;
using Restaurants.Infrastructure.Persistance;
using Restaurants.Infrastructure.Repositories;
using Restaurants.Infrastructure.Seeders;
using Restaurants.Infrastructure.Storage;

namespace Restaurants.Infrastructure.Extensions;

public static class ServiceCollectionExtensions
{
    public static void AddInfrastructure(this IServiceCollection services, IConfiguration configuration) 
    {
        var connectionString = configuration.GetConnectionString("RestaurantsDb");
        var databaseProvider = configuration["Database:Provider"] ?? "SqlServer";
        services.AddDbContext<RestaurantsDbContext>(options =>
        {
            switch (databaseProvider)
            {
                case "SqlServer":
                    options.UseSqlServer(connectionString);
                    break;
                case "PostgreSql":
                    options.UseNpgsql(connectionString, postgres =>
                        postgres.MigrationsAssembly("Restaurants.PostgreSqlMigrations"));
                    break;
                default:
                    throw new InvalidOperationException($"Unsupported database provider '{databaseProvider}'.");
            }
        });

        services.AddIdentityApiEndpoints<User>()
            .AddRoles<IdentityRole>()
            .AddClaimsPrincipalFactory<RestaurantsUserClaimsPrincipalFactory>()
            .AddEntityFrameworkStores<RestaurantsDbContext>();

        services.AddScoped<DatabaseMigrator>();
        services.AddScoped<SampleRestaurantSeeder>();
        services.AddScoped<IdentityRoleSeeder>();
        services.AddScoped<BootstrapAdminProvisioner>();
        services.AddScoped<IStartupDataInitializer, StartupDataInitializer>();
        services.AddScoped<IRestaurantsRepository, RestaurantsRepository>();
        services.AddScoped<IDishesRepository, DishesRepository>();
        services.AddAuthorizationBuilder()
            .AddPolicy(PolicyNames.HasNationality, 
            builder => builder.RequireClaim(AppClaimTypes.Nationality, "German", "Polish"))
            .AddPolicy(PolicyNames.CreatedAtLeast2Restaurants,
            builder => builder.AddRequirements(new CreatedMultipleRestaurantsRequirement(2)));

        services.AddScoped<IRestaurantAuthorizationService, RestaurantAuthorizationService>();
        services.AddScoped<IAuthorizationHandler, CreatedMultipleRestaurantsRequirementHandler>();

        var logoStorageProvider = configuration["Storage:LogoProvider"] ?? "AzureBlobStorage";
        switch (logoStorageProvider)
        {
            case "AzureBlobStorage":
                services.Configure<BlobStorageSettings>(configuration.GetSection("BlobStorage"));
                services.AddScoped<IRestaurantLogoStorage, BlobStorageService>();
                break;
            case "GoogleCloudStorage":
                services.Configure<GoogleCloudStorageSettings>(configuration.GetSection("Storage:GoogleCloud"));
                services.AddSingleton<IRestaurantLogoStorage, GoogleCloudRestaurantLogoStorage>();
                break;
            default:
                throw new InvalidOperationException($"Unsupported logo storage provider '{logoStorageProvider}'.");
        }

        services.AddLocalEatsDataProtection(configuration);
    }
}
