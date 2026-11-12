namespace Restaurants.API.Validation;

public static class ImageFileValidator
{
    public const long MaxFileSize = 5 * 1024 * 1024;
    public const long MaxRequestSize = MaxFileSize + 64 * 1024;

    public static async Task<string?> GetValidatedContentTypeAsync(IFormFile? file, CancellationToken cancellationToken)
    {
        if (file is null || file.Length is <= 0 or > MaxFileSize)
            return null;

        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        var allowed = extension switch
        {
            ".png" when string.Equals(file.ContentType, "image/png", StringComparison.OrdinalIgnoreCase) => "png",
            ".jpg" or ".jpeg" when string.Equals(file.ContentType, "image/jpeg", StringComparison.OrdinalIgnoreCase) => "jpeg",
            ".webp" when string.Equals(file.ContentType, "image/webp", StringComparison.OrdinalIgnoreCase) => "webp",
            _ => null
        };
        if (allowed is null)
            return null;

        var header = new byte[12];
        await using var stream = file.OpenReadStream();
        var read = await stream.ReadAtLeastAsync(header.AsMemory(), header.Length, throwOnEndOfStream: false, cancellationToken: cancellationToken);

        var isValid = allowed switch
        {
            "png" => read >= 8 && header.AsSpan(0, 8).SequenceEqual(new byte[] { 137, 80, 78, 71, 13, 10, 26, 10 }),
            "jpeg" => read >= 3 && header[0] == 255 && header[1] == 216 && header[2] == 255,
            "webp" => read >= 12 && header.AsSpan(0, 4).SequenceEqual("RIFF"u8) && header.AsSpan(8, 4).SequenceEqual("WEBP"u8),
            _ => false
        };

        return isValid ? $"image/{allowed}" : null;
    }
}
