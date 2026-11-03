namespace TechNotes.Application.DTOs.Images;

public sealed class ImageUploadResponse
{
    public string Url { get; set; } = string.Empty;

    public string PublicId { get; set; } = string.Empty;

    public int Width { get; set; }

    public int Height { get; set; }

    public string Format { get; set; } = string.Empty;
}
