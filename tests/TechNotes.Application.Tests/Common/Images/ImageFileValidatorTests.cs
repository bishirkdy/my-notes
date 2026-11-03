using TechNotes.Application.Common.Images;

namespace TechNotes.Application.Tests.Common.Images;

public sealed class ImageFileValidatorTests
{
    private static readonly byte[] PngHeader = [0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A, 0, 0, 0, 0];
    private static readonly byte[] JpegHeader = [0xFF, 0xD8, 0xFF, 0xE0, 0, 0, 0, 0, 0, 0, 0, 0];
    private static readonly byte[] GifHeader = "GIF89a000000"u8.ToArray();
    private static readonly byte[] WebpHeader = "RIFF0000WEBP"u8.ToArray();

    [Theory]
    [MemberData(nameof(ValidImages))]
    public void Validate_AcceptsSupportedImageSignatures(string contentType, byte[] header)
    {
        ImageFileValidator.Validate("image.bin", contentType, 1024, header);
    }

    [Fact]
    public void Validate_RejectsOversizedImage()
    {
        var error = Assert.Throws<ArgumentException>(() =>
            ImageFileValidator.Validate("image.png", "image/png", ImageFileValidator.MaxFileSizeBytes + 1, PngHeader));

        Assert.Contains("10 MB", error.Message);
    }

    [Fact]
    public void Validate_RejectsSvg()
    {
        var header = "<svg xmlns=\"http://www.w3.org/2000/svg\">"u8.ToArray();

        var error = Assert.Throws<ArgumentException>(() =>
            ImageFileValidator.Validate("image.svg", "image/svg+xml", header.Length, header));

        Assert.Contains("JPEG, PNG, WebP, and GIF", error.Message);
    }

    [Fact]
    public void Validate_RejectsMismatchedContentType()
    {
        var error = Assert.Throws<ArgumentException>(() =>
            ImageFileValidator.Validate("image.jpg", "image/jpeg", 1024, PngHeader));

        Assert.Contains("does not match", error.Message);
    }

    [Fact]
    public void Validate_RejectsUnknownFileSignature()
    {
        var error = Assert.Throws<ArgumentException>(() =>
            ImageFileValidator.Validate("image.png", "image/png", 1024, "not-an-image"u8));

        Assert.Contains("not a valid", error.Message);
    }

    [Fact]
    public void Validate_RejectsEmptyFile()
    {
        Assert.Throws<ArgumentException>(() =>
            ImageFileValidator.Validate("image.png", "image/png", 0, []));
    }

    public static TheoryData<string, byte[]> ValidImages => new()
    {
        { "image/png", PngHeader },
        { "image/jpeg", JpegHeader },
        { "image/gif", GifHeader },
        { "image/webp", WebpHeader }
    };
}
