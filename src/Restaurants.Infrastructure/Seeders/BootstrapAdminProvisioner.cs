using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Restaurants.Domain.Constants;
using Restaurants.Domain.Entities;

namespace Restaurants.Infrastructure.Seeders;

internal sealed class BootstrapAdminProvisioner(
    UserManager<User> userManager,
    IConfiguration configuration,
    ILogger<BootstrapAdminProvisioner> logger)
{
    public async Task ProvisionAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var email = configuration["BootstrapAdmin:Email"]?.Trim();
        var password = configuration["BootstrapAdmin:Password"];
        if (string.IsNullOrWhiteSpace(email) && string.IsNullOrWhiteSpace(password))
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
        {
            throw new InvalidOperationException("Both BootstrapAdmin:Email and BootstrapAdmin:Password must be configured together.");
        }

        var admin = await userManager.FindByEmailAsync(email);
        if (admin is null)
        {
            admin = new User { UserName = email, Email = email, EmailConfirmed = true };
            var createResult = await userManager.CreateAsync(admin, password);
            if (!createResult.Succeeded)
            {
                var errorCodes = string.Join("; ", createResult.Errors.Select(error => error.Code));
                throw new InvalidOperationException($"Bootstrap admin creation failed: {errorCodes}");
            }

            logger.LogWarning("Bootstrap admin account created for {AdminEmail}. Remove bootstrap credentials from configuration.", email);
        }

        if (!await userManager.IsInRoleAsync(admin, UserRoles.Admin))
        {
            var roleResult = await userManager.AddToRoleAsync(admin, UserRoles.Admin);
            if (!roleResult.Succeeded)
            {
                throw new InvalidOperationException("Could not assign the bootstrap admin role.");
            }
        }
    }
}
