using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Restaurants.Domain.Exceptions;

namespace Restaurants.Application.User.Commands.UpdateUserDetails;

public class UpdateUserDetailsCommandHandler(ILogger<UpdateUserDetailsCommandHandler> logger,
    IUserContext userContext,
    IUserStore<Domain.Entities.User> userStore) : IRequestHandler<UpdateUserDetailsCommand>
{
    public async Task Handle(UpdateUserDetailsCommand request, CancellationToken cancellationToken)
    {
        var user = userContext.GetCurrentUser();

        logger.LogInformation("Updating profile for user {UserId}", user!.Id);

        var dbUser = await userStore.FindByIdAsync(user!.Id, cancellationToken);

        if (dbUser == null)
        {
            throw new NotFoundException(nameof(Domain.Entities.User), user!.Id);
        }

        dbUser.Nationality = request.Nationality;
        dbUser.DateOfBirth = request.DateOfBirth;

        var result = await userStore.UpdateAsync(dbUser, cancellationToken);
        if (!result.Succeeded)
            throw new InvalidOperationException("The profile update could not be completed.");

    }
}
