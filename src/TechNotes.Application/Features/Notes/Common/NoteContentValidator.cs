using System.Text.Json;
using TechNotes.Application.DTOs.Notes;
using TechNotes.Domain.Enums;

namespace TechNotes.Application.Features.Notes.Common;

public static class NoteContentValidator
{
    public static void Validate(IEnumerable<NoteSectionRequest> sections)
    {
        foreach (var section in sections)
        {
            if (string.IsNullOrWhiteSpace(section.Title))
            {
                throw new ArgumentException("Every section must have a title.");
            }

            foreach (var content in section.Contents)
            {
                if (!Enum.IsDefined(content.Type) || content.Type == 0)
                {
                    throw new ArgumentException($"Content type '{content.Type}' is not supported.");
                }

                ValidateJson(content.Data, "Content data");
                ValidateJson(content.Metadata, "Content metadata");

                if (content.Type == NoteContentType.Image)
                {
                    ValidateImageData(content.Data);
                }
            }

            Validate(section.Children);
        }
    }

    private static void ValidateJson(string? value, string fieldName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return;
        }

        try
        {
            using var document = JsonDocument.Parse(value);
        }
        catch (JsonException)
        {
            throw new ArgumentException($"{fieldName} must contain valid JSON.");
        }
    }

    private static void ValidateImageData(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("Image content must include a URL and public ID.");
        }

        JsonDocument document;
        try
        {
            document = JsonDocument.Parse(value);
        }
        catch (JsonException)
        {
            throw new ArgumentException("Image content must contain valid JSON.");
        }

        using (document)
        {
            if (document.RootElement.ValueKind != JsonValueKind.Object)
            {
                throw new ArgumentException("Image content must be a JSON object.");
            }

            if (!TryGetString(document.RootElement, "url", out var url) ||
                !IsValidImageUrl(url))
            {
                throw new ArgumentException("Image content must include a valid URL.");
            }

            if (!TryGetString(document.RootElement, "publicId", out _))
            {
                throw new ArgumentException("Image content must include a public ID.");
            }

            ValidateOptionalPositiveNumber(document.RootElement, "width");
            ValidateOptionalPositiveNumber(document.RootElement, "height");
        }
    }

    private static bool IsValidImageUrl(string url)
    {
        if (url.StartsWith('/'))
        {
            return !url.StartsWith("//")
                && !url.Contains("..")
                && !url.Contains('\\')
                && url.All(character => !char.IsControl(character));
        }

        if (Uri.TryCreate(url, UriKind.Absolute, out var absolute))
        {
            return absolute.Scheme == Uri.UriSchemeHttps || absolute.Scheme == Uri.UriSchemeHttp;
        }

        return false;
    }

    private static bool TryGetString(JsonElement element, string propertyName, out string value)
    {
        value = string.Empty;
        if (!element.TryGetProperty(propertyName, out var property) ||
            property.ValueKind != JsonValueKind.String)
        {
            return false;
        }

        value = property.GetString()?.Trim() ?? string.Empty;
        return value.Length > 0;
    }

    private static void ValidateOptionalPositiveNumber(JsonElement element, string propertyName)
    {
        if (!element.TryGetProperty(propertyName, out var property) ||
            property.ValueKind == JsonValueKind.Null)
        {
            return;
        }

        if (property.ValueKind != JsonValueKind.Number ||
            !property.TryGetInt32(out var value) ||
            value <= 0)
        {
            throw new ArgumentException($"Image {propertyName} must be a positive number.");
        }
    }
}
