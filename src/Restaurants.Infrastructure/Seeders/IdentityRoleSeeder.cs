using Microsoft.AspNetCore.Identity;
using Restaurants.Domain.Constants;

namespace Restaurants.Infrastructure.Seeders;

internal sealed class IdentityRoleSeeder(RoleManager<IdentityRole> roleManager)
{
    public async Task SeedAsync(CancellationToken cancellationToken)
    {
        foreach (var roleName in new[] { UserRoles.User, UserRoles.Owner, UserRoles.Admin })
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (await roleManager.RoleExistsAsync(roleName))
            {
                continue;
            }

            var result = await roleManager.CreateAsync(new IdentityRole(roleName));
            if (!result.Succeeded)
            {
                throw new InvalidOperationException($"Could not seed role {roleName}.");
            }
        }
    }
}
