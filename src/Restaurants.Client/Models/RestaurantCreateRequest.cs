using System.ComponentModel.DataAnnotations;

namespace Restaurants.Client.Models;

public sealed class RestaurantCreateRequest : IValidatableObject
{
    [Required, MinLength(3), StringLength(100), PlainText]
    public string Name { get; set; } = string.Empty;

    [Required, StringLength(1000), PlainText]
    public string Description { get; set; } = string.Empty;

    [Required, StringLength(50), PlainText]
    public string Category { get; set; } = string.Empty;

    public bool HasDelivery { get; set; }
    [EmailAddress, StringLength(256)]
    public string? ContactEmail { get; set; }

    [StringLength(30), RegularExpression(@"^(?:[0-9+() .-]{7,30})?$")]
    public string? ContactNumber { get; set; }

    [StringLength(100), PlainText]
    public string? City { get; set; }

    [StringLength(150), PlainText]
    public string? Street { get; set; }

    [StringLength(10), RegularExpression(@"^(?:\d{2}-\d{3})?$")]
    public string? PostalCode { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        var allowed = new[] { "Italian", "Mexican", "Japanese", "American", "Indian" };
        if (!allowed.Contains(Category, StringComparer.Ordinal))
            yield return new ValidationResult("Choose a supported cuisine category.", [nameof(Category)]);
    }
}
