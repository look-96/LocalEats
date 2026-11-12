using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Restaurants.Domain.Exceptions;

namespace Restaurants.Application.User.Commands.AssignUserRole;

public class AssignUserRoleCommandHandler(ILogger<AssignUserRoleCommandHandler> logger,
    UserManager<Domain.Entities.User> userManager,
    RoleManager<IdentityRole> roleManager) : IRequestHandler<AssignUserRoleCommand>
{
    public async Task Handle(AssignUserRoleCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Assigning role {RoleName}", request.RoleName);

        var user = await userManager.FindByEmailAsync(request.UserEmail)
            ?? throw new NotFoundException(nameof(Domain.Entities.User), request.UserEmail);

        var role = await roleManager.FindByNameAsync(request.RoleName)
            ?? throw new NotFoundException(nameof(IdentityRole), request.RoleName);

        var result = await userManager.AddToRoleAsync(user, role.Name!);
        if (!result.Succeeded)
            throw new InvalidOperationException("The role assignment could not be completed.");
        logger.LogInformation("Assigned role {RoleName} to user {UserId}", role.Name, user.Id);
    }
}
