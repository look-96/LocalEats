using System.ComponentModel.DataAnnotations;

namespace Restaurants.Client.Models;

public sealed class RestaurantUpdateRequest
{
    [Required, MinLength(3), StringLength(100), PlainText]
    public string Name { get; set; } = string.Empty;

    [Required, StringLength(1000), PlainText]
    public string Description { get; set; } = string.Empty;

    public bool HasDelivery { get; set; }
}
