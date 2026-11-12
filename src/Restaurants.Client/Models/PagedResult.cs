namespace Restaurants.Client.Models;

public sealed class PagedResult<T>
{
    public List<T> Items { get; set; } = [];
    public int TotalPages { get; set; }
    public int TotalItemsCount { get; set; }
    public int ItemsFrom { get; set; }
    public int ItemsTo { get; set; }
}
