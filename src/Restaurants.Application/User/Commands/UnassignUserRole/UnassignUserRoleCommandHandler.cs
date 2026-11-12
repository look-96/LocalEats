using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Restaurants.Application.User.Commands.AssignUserRole;
using Restaurants.Domain.Exceptions;

namespace Restaurants.Application.User.Commands.UnassignUserRole;

public class UnassignUserRoleCommandHandler(ILogger<UnassignUserRoleCommandHandler> logger,
    UserManager<Domain.Entities.User> userManager,
    RoleManager<IdentityRole> roleManager) : IRequestHandler<UnassignUserRoleCommand>
{
    public async Task Handle(UnassignUserRoleCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Unassigning role {RoleName}", request.RoleName);

        var user = await userManager.FindByEmailAsync(request.UserEmail)
            ?? throw new NotFoundException(nameof(Domain.Entities.User), request.UserEmail);

        var role = await roleManager.FindByNameAsync(request.RoleName)
            ?? throw new NotFoundException(nameof(IdentityRole), request.RoleName);

        var result = await userManager.RemoveFromRoleAsync(user, role.Name!);
        if (!result.Succeeded)
            throw new InvalidOperationException("The role removal could not be completed.");
        logger.LogInformation("Removed role {RoleName} from user {UserId}", role.Name, user.Id);
    }
}

