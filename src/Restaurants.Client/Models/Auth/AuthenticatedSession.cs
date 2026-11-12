namespace Restaurants.Client.Models.Auth;

public sealed class AuthenticatedSession
{
    public string? UserId { get; set; }
    public string? Email { get; set; }
    public string[] Roles { get; set; } = [];
}
