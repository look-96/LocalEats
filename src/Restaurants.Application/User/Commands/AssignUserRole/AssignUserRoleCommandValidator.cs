using FluentValidation;
using Restaurants.Domain.Constants;

namespace Restaurants.Application.User.Commands.AssignUserRole;

public sealed class AssignUserRoleCommandValidator : AbstractValidator<AssignUserRoleCommand>
{
    private static readonly string[] AllowedRoles = [UserRoles.User, UserRoles.Owner, UserRoles.Admin];

    public AssignUserRoleCommandValidator()
    {
        RuleFor(command => command.UserEmail)
            .NotEmpty().MaximumLength(256).EmailAddress();
        RuleFor(command => command.RoleName)
            .Must(role => AllowedRoles.Contains(role, StringComparer.Ordinal))
            .WithMessage("Role must be User, Owner, or Admin.");
    }
}
