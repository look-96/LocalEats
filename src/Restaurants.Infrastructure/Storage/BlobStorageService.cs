using Azure.Storage.Blobs;
using Azure.Storage.Sas;
using Microsoft.Extensions.Options;
using Restaurants.Domain.Interfaces;
using Restaurants.Infrastructure.Configuration;

namespace Restaurants.Infrastructure.Storage;

public class BlobStorageService(IOptions<BlobStorageSettings> blobStorageSettingsOptions) : IRestaurantLogoStorage
{
    private readonly BlobStorageSettings _blobStorageSettings = blobStorageSettingsOptions.Value;
    public async Task<string> UploadAsync(Stream data, string fileName, string contentType, CancellationToken cancellationToken = default)
    {
        var blobServiceClient = new BlobServiceClient(_blobStorageSettings.ConnectionString);
        var containerClient = blobServiceClient.GetBlobContainerClient(_blobStorageSettings.LogosContainerName);

        var blobClient = containerClient.GetBlobClient(fileName);

        await blobClient.UploadAsync(data, new Azure.Storage.Blobs.Models.BlobUploadOptions
        {
            HttpHeaders = new Azure.Storage.Blobs.Models.BlobHttpHeaders { ContentType = contentType }
        }, cancellationToken);

        var blobUrl = blobClient.Uri.ToString();

        return blobUrl;
    }

    public Task<string?> CreateReadUrlAsync(string? blobUrl, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (blobUrl == null) return Task.FromResult<string?>(null);

        var sasBuilder = new BlobSasBuilder()
        {
            BlobContainerName = _blobStorageSettings.LogosContainerName,
            Resource = "b",
            StartsOn = DateTimeOffset.UtcNow,
            ExpiresOn = DateTimeOffset.UtcNow.AddMinutes(30),
            BlobName = GetBlobNameFromUrl(blobUrl)
        };

        sasBuilder.SetPermissions(BlobSasPermissions.Read);

        var blobServiceClient = new BlobServiceClient(_blobStorageSettings.ConnectionString);

        var sasToken = sasBuilder
            .ToSasQueryParameters(new Azure.Storage.StorageSharedKeyCredential(blobServiceClient.AccountName, _blobStorageSettings.AccountKey))
            .ToString();

        return Task.FromResult<string?>($"{blobUrl}?{sasToken}");
    }

    private string GetBlobNameFromUrl(string blobUrl)
    {
        var uri = new Uri(blobUrl);
        return uri.Segments.Last();
    }
    
}
