namespace Restaurants.Client.Models.Auth;

public sealed class TokenSession
{
    public string AccessToken { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
    public long ExpiresAtUnixMilliseconds { get; set; }
}
