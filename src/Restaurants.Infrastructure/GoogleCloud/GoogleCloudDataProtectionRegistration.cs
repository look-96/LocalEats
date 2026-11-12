using Google.Cloud.Kms.V1;
using Google.Cloud.Storage.V1;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.DataProtection.KeyManagement;
using Microsoft.AspNetCore.DataProtection.Repositories;
using Microsoft.AspNetCore.DataProtection.XmlEncryption;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Restaurants.Infrastructure.GoogleCloud;

internal static class GoogleCloudDataProtectionRegistration
{
    public static void AddLocalEatsDataProtection(this IServiceCollection services, IConfiguration configuration)
    {
        var provider = configuration["DataProtection:Provider"];
        var isCloudRun = !string.IsNullOrWhiteSpace(configuration["K_SERVICE"]) ||
                         !string.IsNullOrWhiteSpace(configuration["K_REVISION"]);

        if (string.IsNullOrWhiteSpace(provider))
        {
            if (isCloudRun)
                throw new InvalidOperationException("DataProtection:Provider must be GoogleCloudKms when hosted on Cloud Run.");
            provider = "Local";
        }

        if (isCloudRun && !string.Equals(provider, "GoogleCloudKms", StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("Cloud Run must use Google Cloud Storage and Cloud KMS for Data Protection keys.");

        if (string.Equals(provider, "Local", StringComparison.OrdinalIgnoreCase))
        {
            services.AddDataProtection().SetApplicationName("LocalEats");
            return;
        }

        if (!string.Equals(provider, "GoogleCloudKms", StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("DataProtection:Provider must be Local or GoogleCloudKms.");

        var bucket = configuration["Storage:GoogleCloud:Bucket"];
        var objectPrefix = ReadPrefix(configuration["DataProtection:ObjectPrefix"] ?? "data-protection/localeats");
        var kmsKeyName = configuration["DataProtection:KmsKeyName"];
        if (string.IsNullOrWhiteSpace(bucket))
            throw new InvalidOperationException("Storage:GoogleCloud:Bucket must be configured for Cloud Run Data Protection.");
        if (!IsValidKmsKeyName(kmsKeyName))
            throw new InvalidOperationException("DataProtection:KmsKeyName must identify a Google Cloud KMS crypto key.");

        services.AddSingleton(StorageClient.Create());
        services.AddSingleton(KeyManagementServiceClient.Create());
        services.AddSingleton<IXmlRepository>(serviceProvider =>
            new GoogleCloudStorageXmlRepository(
                serviceProvider.GetRequiredService<StorageClient>(), bucket, objectPrefix));
        services.AddSingleton<IXmlEncryptor>(serviceProvider =>
            new GoogleCloudKmsXmlEncryptor(
                serviceProvider.GetRequiredService<KeyManagementServiceClient>(), kmsKeyName!));
        services.AddSingleton<IConfigureOptions<KeyManagementOptions>, GoogleCloudDataProtectionOptionsSetup>();
        services.AddDataProtection().SetApplicationName("LocalEats");
    }

    private static string ReadPrefix(string value)
    {
        var segments = value.Split('/', StringSplitOptions.RemoveEmptyEntries);
        if (segments.Length == 0 || value.StartsWith('/') || value.EndsWith('/') ||
            segments.Any(segment => segment is "." or ".." ||
                                    segment.Any(character => !(char.IsAsciiLetterOrDigit(character) || character is '-' or '_'))))
            throw new InvalidOperationException("DataProtection:ObjectPrefix must contain safe alphanumeric path segments.");

        return string.Join('/', segments);
    }

    private static bool IsValidKmsKeyName(string? keyName)
    {
        if (string.IsNullOrWhiteSpace(keyName))
            return false;

        var parts = keyName.Split('/');
        return parts.Length == 8 &&
               parts[0] == "projects" && IsSafeResourceName(parts[1]) &&
               parts[2] == "locations" && IsSafeResourceName(parts[3]) &&
               parts[4] == "keyRings" && IsSafeResourceName(parts[5]) &&
               parts[6] == "cryptoKeys" && IsSafeResourceName(parts[7]);
    }

    private static bool IsSafeResourceName(string value) =>
        value.Length is >= 1 and <= 63 &&
        value.All(character => char.IsAsciiLetterOrDigit(character) || character is '-' or '_') &&
        char.IsAsciiLetterOrDigit(value[0]) && char.IsAsciiLetterOrDigit(value[^1]);
}
