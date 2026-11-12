using System.ComponentModel.DataAnnotations;

namespace Restaurants.Client.Models;

public sealed class DishCreateRequest
{
    [Required, StringLength(100), PlainText]
    public string Name { get; set; } = string.Empty;

    [Required, StringLength(1000), PlainText]
    public string Description { get; set; } = string.Empty;

    [Range(typeof(decimal), "0", "1000000")]
    public decimal Price { get; set; }

    [Range(0, 100000)]
    public int? KiloCalories { get; set; }
}
