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
}
