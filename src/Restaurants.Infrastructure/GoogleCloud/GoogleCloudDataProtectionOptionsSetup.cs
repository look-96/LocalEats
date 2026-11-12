using Microsoft.AspNetCore.DataProtection.KeyManagement;
using Microsoft.AspNetCore.DataProtection.Repositories;
using Microsoft.AspNetCore.DataProtection.XmlEncryption;
using Microsoft.Extensions.Options;

namespace Restaurants.Infrastructure.GoogleCloud;

internal sealed class GoogleCloudDataProtectionOptionsSetup(
    IXmlRepository xmlRepository,
    IXmlEncryptor xmlEncryptor) : IConfigureOptions<KeyManagementOptions>
{
    public void Configure(KeyManagementOptions options)
    {
        options.XmlRepository = xmlRepository;
        options.XmlEncryptor = xmlEncryptor;
    }
}
