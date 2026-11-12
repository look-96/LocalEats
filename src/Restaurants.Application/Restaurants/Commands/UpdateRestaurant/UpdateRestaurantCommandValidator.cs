using FluentValidation;
using Restaurants.Application.Common.Validation;

namespace Restaurants.Application.Restaurants.Commands.UpdateRestaurant;

public sealed class UpdateRestaurantCommandValidator : AbstractValidator<UpdateRestaurantCommand>
{
    public UpdateRestaurantCommandValidator()
    {
        RuleFor(command => command.Name)
            .NotEmpty().MinimumLength(3).MaximumLength(100)
            .Must(PlainTextRules.IsPlainText).WithMessage("Name must be plain text.");

        RuleFor(command => command.Description)
            .NotEmpty().MaximumLength(1000)
            .Must(PlainTextRules.IsPlainText).WithMessage("Description must be plain text.");
    }
}
