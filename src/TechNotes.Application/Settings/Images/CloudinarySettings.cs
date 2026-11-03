namespace TechNotes.Application.Settings.Images;

public sealed class CloudinarySettings
{
    public const string SectionName = "Cloudinary";

    public string CloudName { get; set; } = string.Empty;

    public string ApiKey { get; set; } = string.Empty;

    public string ApiSecret { get; set; } = string.Empty;

    public string Folder { get; set; } = "technotes";
}
