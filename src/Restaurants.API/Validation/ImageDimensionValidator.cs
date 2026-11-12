namespace Restaurants.API.Validation;

internal static class ImageDimensionValidator
{
    private const int MaximumJpegHeaderBytes = 1024 * 1024;

    public static async Task<bool> HasAllowedDimensionsAsync(
        IFormFile file,
        string imageType,
        CancellationToken cancellationToken)
    {
        await using var stream = file.OpenReadStream();
        return imageType switch
        {
            "png" => await HasPngDimensionsAsync(stream, cancellationToken),
            "jpeg" => await HasJpegDimensionsAsync(stream, cancellationToken),
            "webp" => await HasWebpDimensionsAsync(stream, cancellationToken),
            _ => false
        };
    }

    private static async Task<bool> HasPngDimensionsAsync(Stream stream, CancellationToken cancellationToken)
    {
        var header = new byte[24];
        if (await stream.ReadAtLeastAsync(header, header.Length, false, cancellationToken) < header.Length ||
            !header.AsSpan(8, 4).SequenceEqual(new byte[] { 0, 0, 0, 13 }) ||
            !header.AsSpan(12, 4).SequenceEqual("IHDR"u8))
            return false;

        var width = ReadBigEndianUInt32(header, 16);
        var height = ReadBigEndianUInt32(header, 20);
        return HasAllowedPixelCount(width, height);
    }

    private static async Task<bool> HasJpegDimensionsAsync(Stream stream, CancellationToken cancellationToken)
    {
        var singleByteBuffer = new byte[1];
        if (await ReadByteAsync(stream, singleByteBuffer, cancellationToken) != 0xFF ||
            await ReadByteAsync(stream, singleByteBuffer, cancellationToken) != 0xD8)
            return false;

        var scannedBytes = 2;
        while (scannedBytes < MaximumJpegHeaderBytes)
        {
            var value = await ReadByteAsync(stream, singleByteBuffer, cancellationToken);
            scannedBytes++;
            if (value < 0)
                return false;
            if (value != 0xFF)
                continue;

            int marker;
            do
            {
                marker = await ReadByteAsync(stream, singleByteBuffer, cancellationToken);
                scannedBytes++;
            } while (marker == 0xFF && scannedBytes < MaximumJpegHeaderBytes);

            if (marker < 0 || marker is 0xD9 or 0xDA)
                return false;
            if (marker is 0x01 or >= 0xD0 and <= 0xD7)
                continue;

            var lengthHigh = await ReadByteAsync(stream, singleByteBuffer, cancellationToken);
            var lengthLow = await ReadByteAsync(stream, singleByteBuffer, cancellationToken);
            scannedBytes += 2;
            if (lengthHigh < 0 || lengthLow < 0)
                return false;

            var segmentLength = (lengthHigh << 8) | lengthLow;
            if (segmentLength < 2 || scannedBytes + segmentLength - 2 > MaximumJpegHeaderBytes)
                return false;

            if (IsStartOfFrame(marker))
            {
                if (segmentLength < 7)
                    return false;
                _ = await ReadByteAsync(stream, singleByteBuffer, cancellationToken);
                var height = await ReadBigEndianUInt16Async(stream, singleByteBuffer, cancellationToken);
                var width = await ReadBigEndianUInt16Async(stream, singleByteBuffer, cancellationToken);
                return width.HasValue && height.HasValue && HasAllowedPixelCount(width.Value, height.Value);
            }

            if (!await SkipAsync(stream, segmentLength - 2, cancellationToken))
                return false;
            scannedBytes += segmentLength - 2;
        }

        return false;
    }

    private static async Task<bool> HasWebpDimensionsAsync(Stream stream, CancellationToken cancellationToken)
    {
        var header = new byte[30];
        if (await stream.ReadAtLeastAsync(header, header.Length, false, cancellationToken) < header.Length ||
            !header.AsSpan(0, 4).SequenceEqual("RIFF"u8) ||
            !header.AsSpan(8, 4).SequenceEqual("WEBP"u8))
            return false;

        uint width;
        uint height;
        if (header.AsSpan(12, 4).SequenceEqual("VP8X"u8))
        {
            width = 1u + ReadLittleEndianUInt24(header, 24);
            height = 1u + ReadLittleEndianUInt24(header, 27);
        }
        else if (header.AsSpan(12, 4).SequenceEqual("VP8 "u8))
        {
            if (!header.AsSpan(23, 3).SequenceEqual(new byte[] { 0x9D, 0x01, 0x2A }))
                return false;
            width = (uint)((header[26] | header[27] << 8) & 0x3FFF);
            height = (uint)((header[28] | header[29] << 8) & 0x3FFF);
        }
        else if (header.AsSpan(12, 4).SequenceEqual("VP8L"u8))
        {
            if (header[20] != 0x2F)
                return false;
            width = 1u + (uint)(header[21] | ((header[22] & 0x3F) << 8));
            height = 1u + (uint)((header[22] >> 6) | (header[23] << 2) | ((header[24] & 0x0F) << 10));
        }
        else
        {
            return false;
        }

        return HasAllowedPixelCount(width, height);
    }

    private static bool HasAllowedPixelCount(uint width, uint height) =>
        width > 0 && height > 0 && (ulong)width * height <= ImageFileValidator.MaxPixelCount;

    private static bool IsStartOfFrame(int marker) => marker is
        0xC0 or 0xC1 or 0xC2 or 0xC3 or 0xC5 or 0xC6 or 0xC7 or
        0xC9 or 0xCA or 0xCB or 0xCD or 0xCE or 0xCF;

    private static uint ReadBigEndianUInt32(byte[] bytes, int offset) =>
        ((uint)bytes[offset] << 24) | ((uint)bytes[offset + 1] << 16) |
        ((uint)bytes[offset + 2] << 8) | bytes[offset + 3];

    private static uint ReadLittleEndianUInt24(byte[] bytes, int offset) =>
        (uint)(bytes[offset] | (bytes[offset + 1] << 8) | (bytes[offset + 2] << 16));

    private static async Task<ushort?> ReadBigEndianUInt16Async(
        Stream stream,
        byte[] singleByteBuffer,
        CancellationToken cancellationToken)
    {
        var high = await ReadByteAsync(stream, singleByteBuffer, cancellationToken);
        var low = await ReadByteAsync(stream, singleByteBuffer, cancellationToken);
        return high < 0 || low < 0 ? null : (ushort)((high << 8) | low);
    }

    private static async Task<bool> SkipAsync(Stream stream, int bytesToSkip, CancellationToken cancellationToken)
    {
        var buffer = new byte[4096];
        while (bytesToSkip > 0)
        {
            var count = await stream.ReadAsync(buffer.AsMemory(0, Math.Min(buffer.Length, bytesToSkip)), cancellationToken);
            if (count == 0)
                return false;
            bytesToSkip -= count;
        }

        return true;
    }

    private static async ValueTask<int> ReadByteAsync(
        Stream stream,
        byte[] singleByteBuffer,
        CancellationToken cancellationToken)
    {
        return await stream.ReadAsync(singleByteBuffer, cancellationToken) == 1 ? singleByteBuffer[0] : -1;
    }
}
