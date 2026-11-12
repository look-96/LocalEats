using FluentValidation;
using Restaurants.Application.Common.Validation;

namespace Restaurants.Application.Dishes.Commands.CreateDish;

public sealed class CreateDishCommandValidator : AbstractValidator<CreateDishCommand>
{
    public CreateDishCommandValidator()
    {
        RuleFor(dish => dish.Name)
            .NotEmpty().MaximumLength(100)
            .Must(PlainTextRules.IsPlainText).WithMessage("Dish name must be plain text.");

        RuleFor(dish => dish.Description)
            .NotEmpty().MaximumLength(1000)
            .Must(PlainTextRules.IsPlainText).WithMessage("Dish description must be plain text.");

        RuleFor(dish => dish.Price)
            .GreaterThanOrEqualTo(0)
            .LessThanOrEqualTo(1_000_000)
            .WithMessage("Price must be between 0 and 1,000,000.");

        RuleFor(dish => dish.KiloCalories)
            .GreaterThanOrEqualTo(0)
            .LessThanOrEqualTo(100_000)
            .When(dish => dish.KiloCalories.HasValue)
            .WithMessage("Calories must be between 0 and 100,000.");
    }
}
