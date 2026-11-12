using Google.Cloud.Kms.V1;
using Google.Protobuf;
using Microsoft.AspNetCore.DataProtection.XmlEncryption;
using System.Text;
using System.Xml.Linq;

namespace Restaurants.Infrastructure.GoogleCloud;

public sealed class GoogleCloudKmsXmlDecryptor : IXmlDecryptor
{
    private static readonly Lazy<KeyManagementServiceClient> Client =
        new(KeyManagementServiceClient.Create, LazyThreadSafetyMode.ExecutionAndPublication);

    public XElement Decrypt(XElement encryptedElement)
    {
        if (encryptedElement.Name != "googleCloudKmsEncryptedKey")
            throw new InvalidOperationException("The Data Protection key XML has an unsupported encryption format.");

        var keyName = encryptedElement.Attribute("keyName")?.Value;
        var ciphertextText = encryptedElement.Value;
        if (!IsValidKeyName(keyName) || string.IsNullOrWhiteSpace(ciphertextText))
            throw new InvalidOperationException("The Data Protection key XML is incomplete.");

        byte[] ciphertext;
        try
        {
            ciphertext = Convert.FromBase64String(ciphertextText);
        }
        catch (FormatException exception)
        {
            throw new InvalidOperationException("The Data Protection key XML contains invalid ciphertext.", exception);
        }

        var result = Client.Value.Decrypt(new DecryptRequest
        {
            Name = keyName,
            Ciphertext = ByteString.CopyFrom(ciphertext)
        });

        return XElement.Parse(Encoding.UTF8.GetString(result.Plaintext.ToByteArray()));
    }

    private static bool IsValidKeyName(string? keyName)
    {
        if (string.IsNullOrWhiteSpace(keyName))
            return false;

        var parts = keyName.Split('/');
        if (parts.Length != 8 || parts[0] != "projects" || parts[2] != "locations" ||
            parts[4] != "keyRings" || parts[6] != "cryptoKeys")
            return false;

        return parts.Where((_, index) => index is 1 or 3 or 5 or 7)
            .All(part => part.Length is >= 1 and <= 63 &&
                         part.All(character => char.IsAsciiLetterOrDigit(character) || character is '-' or '_') &&
                         char.IsAsciiLetterOrDigit(part[0]) && char.IsAsciiLetterOrDigit(part[^1]));
    }
}
