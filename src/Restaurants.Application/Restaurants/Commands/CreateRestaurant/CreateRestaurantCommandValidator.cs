using FluentValidation;
using Restaurants.Application.Common.Validation;

namespace Restaurants.Application.Restaurants.Commands.CreateRestaurant;

public sealed class CreateRestaurantCommandValidator : AbstractValidator<CreateRestaurantCommand>
{
    private static readonly string[] ValidCategories = ["Italian", "Mexican", "Japanese", "American", "Indian"];

    public CreateRestaurantCommandValidator()
    {
        RuleFor(command => command.Name)
            .NotEmpty().MaximumLength(100).MinimumLength(3)
            .Must(PlainTextRules.IsPlainText).WithMessage("Name must be plain text.");

        RuleFor(command => command.Description)
            .NotEmpty().MaximumLength(1000)
            .Must(PlainTextRules.IsPlainText).WithMessage("Description must be plain text.");

        RuleFor(command => command.Category)
            .Must(category => ValidCategories.Contains(category, StringComparer.Ordinal))
            .WithMessage("Choose one of the supported cuisine categories.");

        RuleFor(command => command.ContactEmail)
            .EmailAddress().MaximumLength(256)
            .When(command => !string.IsNullOrWhiteSpace(command.ContactEmail));

        RuleFor(command => command.ContactNumber)
            .MaximumLength(30)
            .Matches(@"^[0-9+() .-]{7,30}$")
            .When(command => !string.IsNullOrWhiteSpace(command.ContactNumber));

        RuleFor(command => command.City)
            .MaximumLength(100).Must(PlainTextRules.IsPlainText);
        RuleFor(command => command.Street)
            .MaximumLength(150).Must(PlainTextRules.IsPlainText);
        RuleFor(command => command.PostalCode)
            .MaximumLength(10)
            .Matches(@"^\d{2}-\d{3}$")
            .When(command => !string.IsNullOrWhiteSpace(command.PostalCode));
    }
}
