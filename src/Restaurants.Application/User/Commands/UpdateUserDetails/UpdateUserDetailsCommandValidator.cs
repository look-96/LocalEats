using FluentValidation;
using Restaurants.Application.Common.Validation;

namespace Restaurants.Application.User.Commands.UpdateUserDetails;

public sealed class UpdateUserDetailsCommandValidator : AbstractValidator<UpdateUserDetailsCommand>
{
    public UpdateUserDetailsCommandValidator()
    {
        RuleFor(command => command.Nationality)
            .MaximumLength(50)
            .Must(PlainTextRules.IsPlainText)
            .When(command => command.Nationality is not null);

        RuleFor(command => command.DateOfBirth)
            .Must(value => !value.HasValue ||
                           (value.Value >= new DateOnly(1900, 1, 1) && value.Value <= DateOnly.FromDateTime(DateTime.UtcNow)))
            .WithMessage("Date of birth must be between 1 January 1900 and today.");
    }
}
