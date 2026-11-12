using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using System.Security.Claims;
using Restaurants.Application.User.Commands.AssignUserRole;
using Restaurants.Application.User.Commands.UnassignUserRole;
using Restaurants.Application.User.Commands.UpdateUserDetails;
using Restaurants.Domain.Constants;

namespace Restaurants.API.Controllers;

[ApiController]
[EnableRateLimiting("IdentityByIp")]
[Route("api/identity")]
public class IdentityController(IMediator mediator, ILogger<IdentityController> logger) : ControllerBase
{
    [HttpPatch("user")]
    [Authorize]
    public async Task<IActionResult> UpdateUserDetails(UpdateUserDetailsCommand command)
    {
        await mediator.Send(command);
        return NoContent();
    }

    [HttpPost("userRole")]
    [Authorize(Roles = UserRoles.Admin)]
    public async Task<IActionResult> AssignUserRole(AssignUserRoleCommand command)
    {
        await mediator.Send(command);
        logger.LogWarning("Admin {ActorId} assigned role {RoleName} to account {TargetEmail}", User.FindFirstValue(ClaimTypes.NameIdentifier), command.RoleName, command.UserEmail);
        return NoContent();
    }

    [HttpDelete("userRole")]
    [Authorize(Roles = UserRoles.Admin)]
    public async Task<IActionResult> UnassignUserRole(UnassignUserRoleCommand command)
    {
        await mediator.Send(command);
        logger.LogWarning("Admin {ActorId} removed role {RoleName} from account {TargetEmail}", User.FindFirstValue(ClaimTypes.NameIdentifier), command.RoleName, command.UserEmail);
        return NoContent();
    }

}