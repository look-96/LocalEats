using Google.Cloud.Kms.V1;
using Google.Protobuf;
using Microsoft.AspNetCore.DataProtection.XmlEncryption;
using System.Text;
using System.Xml.Linq;

namespace Restaurants.Infrastructure.GoogleCloud;

internal sealed class GoogleCloudKmsXmlEncryptor(
    KeyManagementServiceClient keyManagementServiceClient,
    string keyName) : IXmlEncryptor
{
    private const int MaximumPlaintextBytes = 60 * 1024;

    public EncryptedXmlInfo Encrypt(XElement plaintextElement)
    {
        var plaintext = Encoding.UTF8.GetBytes(plaintextElement.ToString(SaveOptions.DisableFormatting));
        if (plaintext.Length > MaximumPlaintextBytes)
            throw new InvalidOperationException("The Data Protection key XML exceeds the supported Cloud KMS payload size.");

        var result = keyManagementServiceClient.Encrypt(new EncryptRequest
        {
            Name = keyName,
            Plaintext = ByteString.CopyFrom(plaintext)
        });

        var encryptedElement = new XElement(
            "googleCloudKmsEncryptedKey",
            new XAttribute("keyName", keyName),
            Convert.ToBase64String(result.Ciphertext.ToByteArray()));

        return new EncryptedXmlInfo(encryptedElement, typeof(GoogleCloudKmsXmlDecryptor));
    }
}
