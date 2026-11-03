namespace TechNotes.Application.Common.Images;

public static class ImageFileValidator
{
    public const long MaxFileSizeBytes = 10 * 1024 * 1024;

    private static readonly HashSet<string> AllowedContentTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "image/jpeg",
        "image/png",
        "image/webp",
        "image/gif"
    };

    public static void Validate(
        string? fileName,
        string? contentType,
        long length,
        ReadOnlySpan<byte> header)
    {
        if (string.IsNullOrWhiteSpace(fileName))
        {
            throw new ArgumentException("An image file is required.");
        }

        if (length <= 0)
        {
            throw new ArgumentException("The image file is empty.");
        }

        if (length > MaxFileSizeBytes)
        {
            throw new ArgumentException("The image must be 10 MB or smaller.");
        }

        if (string.IsNullOrWhiteSpace(contentType) || !AllowedContentTypes.Contains(contentType))
        {
            throw new ArgumentException("Only JPEG, PNG, WebP, and GIF images are supported.");
        }

        var detectedContentType = DetectContentType(header);
        if (detectedContentType is null)
        {
            throw new ArgumentException("The uploaded file is not a valid JPEG, PNG, WebP, or GIF image.");
        }

        if (!string.Equals(detectedContentType, contentType, StringComparison.OrdinalIgnoreCase))
        {
            throw new ArgumentException("The image content does not match its declared content type.");
        }
    }

    private static string? DetectContentType(ReadOnlySpan<byte> header)
    {
        if (header.Length >= 3 &&
            header[0] == 0xFF &&
            header[1] == 0xD8 &&
            header[2] == 0xFF)
        {
            return "image/jpeg";
        }

        ReadOnlySpan<byte> pngSignature = [0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A];
        if (header.Length >= pngSignature.Length && header[..pngSignature.Length].SequenceEqual(pngSignature))
        {
            return "image/png";
        }

        if (header.Length >= 6 &&
            (header[..6].SequenceEqual("GIF87a"u8) || header[..6].SequenceEqual("GIF89a"u8)))
        {
            return "image/gif";
        }

        if (header.Length >= 12 &&
            header[..4].SequenceEqual("RIFF"u8) &&
            header.Slice(8, 4).SequenceEqual("WEBP"u8))
        {
            return "image/webp";
        }

        return null;
    }
}
