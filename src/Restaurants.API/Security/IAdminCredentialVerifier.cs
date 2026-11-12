namespace Restaurants.API.Security;

public interface IAdminCredentialVerifier
{
    bool IsValid(string? credential);
}
