using Google.Apis.Auth.OAuth2;
using Google.Cloud.Storage.V1;
using Microsoft.Extensions.Options;
using Restaurants.Domain.Interfaces;
using Restaurants.Infrastructure.Configuration;

namespace Restaurants.Infrastructure.Storage;

public sealed class GoogleCloudRestaurantLogoStorage : IRestaurantLogoStorage
{
    private static readonly HashSet<string> AllowedExtensions = [".jpg", ".jpeg", ".png", ".webp"];
    private readonly StorageClient _storageClient;
    private readonly UrlSigner _urlSigner;
    private readonly GoogleCloudStorageSettings _settings;

    public GoogleCloudRestaurantLogoStorage(IOptions<GoogleCloudStorageSettings> options)
    {
        _settings = options.Value;
        if (string.IsNullOrWhiteSpace(_settings.Bucket))
            throw new InvalidOperationException("Storage:GoogleCloud:Bucket must be configured.");
        if (string.IsNullOrWhiteSpace(_settings.LogoPrefix) || _settings.LogoPrefix.Contains("..", StringComparison.Ordinal))
            throw new InvalidOperationException("Storage:GoogleCloud:LogoPrefix is invalid.");
        if (_settings.SignedUrlLifetimeMinutes is < 1 or > 60)
            throw new InvalidOperationException("Storage:GoogleCloud:SignedUrlLifetimeMinutes must be between 1 and 60.");

        var credential = GoogleCredential.GetApplicationDefault();
        _storageClient = StorageClient.Create(credential);
        _urlSigner = UrlSigner.FromCredential(credential);
    }

    public async Task<string> UploadAsync(
        Stream data,
        string fileName,
        string contentType,
        CancellationToken cancellationToken = default)
    {
        var safeName = ValidateFileName(fileName);
        if (contentType is not ("image/jpeg" or "image/png" or "image/webp"))
            throw new ArgumentException("Unsupported restaurant logo media type.", nameof(contentType));

        var objectName = $"{_settings.LogoPrefix.TrimEnd('/')}/{safeName}";
        await _storageClient.UploadObjectAsync(
            _settings.Bucket,
            objectName,
            contentType,
            data,
            new UploadObjectOptions { IfGenerationMatch = 0 },
            cancellationToken);

        return objectName;
    }

    public async Task<string?> CreateReadUrlAsync(
        string? storageKey,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(storageKey))
            return null;

        var objectName = ValidateStorageKey(storageKey);
        return await _urlSigner.SignAsync(
            _settings.Bucket,
            objectName,
            TimeSpan.FromMinutes(_settings.SignedUrlLifetimeMinutes),
            signingVersion: SigningVersion.V4,
            cancellationToken: cancellationToken);
    }

    private string ValidateStorageKey(string storageKey)
    {
        var expectedPrefix = $"{_settings.LogoPrefix.TrimEnd('/')}/";
        if (!storageKey.StartsWith(expectedPrefix, StringComparison.Ordinal))
            throw new ArgumentException("Invalid restaurant logo storage key.", nameof(storageKey));

        return $"{expectedPrefix}{ValidateFileName(storageKey[expectedPrefix.Length..])}";
    }

    private static string ValidateFileName(string fileName)
    {
        if (string.IsNullOrWhiteSpace(fileName) || fileName != Path.GetFileName(fileName) ||
            fileName.Contains('/') || fileName.Contains('\\'))
            throw new ArgumentException("Invalid restaurant logo file name.", nameof(fileName));

        var extension = Path.GetExtension(fileName).ToLowerInvariant();
        if (!AllowedExtensions.Contains(extension))
            throw new ArgumentException("Invalid restaurant logo file extension.", nameof(fileName));

        return fileName;
    }
}
