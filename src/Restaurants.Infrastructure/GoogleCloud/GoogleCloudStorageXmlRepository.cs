using Google.Cloud.Storage.V1;
using Microsoft.AspNetCore.DataProtection.Repositories;
using System.Xml.Linq;

namespace Restaurants.Infrastructure.GoogleCloud;

internal sealed class GoogleCloudStorageXmlRepository(
    StorageClient storageClient,
    string bucket,
    string objectPrefix) : IXmlRepository
{
    public IReadOnlyCollection<XElement> GetAllElements()
    {
        var elements = new List<XElement>();
        foreach (var storageObject in storageClient.ListObjects(bucket, $"{objectPrefix}/"))
        {
            if (!storageObject.Name.EndsWith(".xml", StringComparison.OrdinalIgnoreCase))
                continue;

            using var stream = new MemoryStream();
            storageClient.DownloadObject(storageObject, stream);
            stream.Position = 0;
            elements.Add(XElement.Load(stream));
        }

        return elements.AsReadOnly();
    }

    public void StoreElement(XElement element, string friendlyName)
    {
        var fileName = Path.GetFileName(friendlyName);
        if (string.IsNullOrWhiteSpace(fileName) || fileName != friendlyName ||
            fileName.Any(character => !(char.IsAsciiLetterOrDigit(character) || character is '-' or '_' or '.')))
            throw new InvalidOperationException("Data Protection supplied an invalid key-ring object name.");

        var objectName = $"{objectPrefix}/{fileName}.xml";
        var content = System.Text.Encoding.UTF8.GetBytes(element.ToString(SaveOptions.DisableFormatting));
        using var stream = new MemoryStream(content, writable: false);
        storageClient.UploadObject(
            bucket,
            objectName,
            "application/xml",
            stream,
            new UploadObjectOptions { IfGenerationMatch = 0 });
    }
}
