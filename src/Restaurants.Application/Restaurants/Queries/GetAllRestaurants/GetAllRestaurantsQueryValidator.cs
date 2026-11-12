using FluentValidation;
using Restaurants.Application.Common.Validation;
using Restaurants.Application.Restaurants.Dtos;

namespace Restaurants.Application.Restaurants.Queries.GetAllRestaurants;

public sealed class GetAllRestaurantsQueryValidator : AbstractValidator<GetAllRestaurantsQuery>
{
    private static readonly int[] AllowedPageSizes = [5, 10, 15, 30];
    private static readonly string[] AllowedSortColumns =
    [
        nameof(RestaurantDto.Name),
        nameof(RestaurantDto.Category),
        nameof(RestaurantDto.Description)
    ];

    public GetAllRestaurantsQueryValidator()
    {
        RuleFor(query => query.PageNumber)
            .InclusiveBetween(1, 1_000_000);

        RuleFor(query => query.PageSize)
            .Must(AllowedPageSizes.Contains)
            .WithMessage($"Page size must be one of [{string.Join(",", AllowedPageSizes)}].");

        RuleFor(query => query.SortBy)
            .Must(value => value is null || AllowedSortColumns.Contains(value, StringComparer.Ordinal))
            .WithMessage($"Sort by must be one of [{string.Join(",", AllowedSortColumns)}].");

        RuleFor(query => query.SortDirection)
            .IsInEnum();

        RuleFor(query => query.SearchPhrase)
            .MaximumLength(100)
            .Must(PlainTextRules.IsPlainText)
            .WithMessage("Search text must be plain text and no longer than 100 characters.");
    }
}
