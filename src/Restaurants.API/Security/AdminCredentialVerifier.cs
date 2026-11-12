using System.Security.Cryptography;
using System.Text;

namespace Restaurants.API.Security;

internal sealed class AdminCredentialVerifier(IConfiguration configuration) : IAdminCredentialVerifier
{
    public bool IsValid(string? credential)
    {
        var configuredHash = configuration["Admin:DestructiveCommandCredentialHash"];
        if (string.IsNullOrWhiteSpace(configuredHash) || string.IsNullOrWhiteSpace(credential))
        {
            return false;
        }

        byte[] expectedHash;
        try
        {
            expectedHash = Convert.FromHexString(configuredHash);
        }
        catch (FormatException)
        {
            return false;
        }

        if (expectedHash.Length != SHA256.HashSizeInBytes)
        {
            return false;
        }

        var suppliedHash = SHA256.HashData(Encoding.UTF8.GetBytes(credential));
        return CryptographicOperations.FixedTimeEquals(suppliedHash, expectedHash);
    }
}
