using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Restaurants.Infrastructure.Extensions;
using Restaurants.Maintenance.Commands;

if (args.Length != 1 || !string.Equals(args[0], "migrate-postgresql", StringComparison.Ordinal))
{
    Console.Error.WriteLine("Usage: Restaurants.Maintenance migrate-postgresql");
    return 2;
}

var builder = Host.CreateApplicationBuilder(Array.Empty<string>());
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddScoped<PostgreSqlMigrationCommand>();

using var host = builder.Build();
using var scope = host.Services.CreateScope();
var command = scope.ServiceProvider.GetRequiredService<PostgreSqlMigrationCommand>();
await command.RunAsync(CancellationToken.None);
return 0;
