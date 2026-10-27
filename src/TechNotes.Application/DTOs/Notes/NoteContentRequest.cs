using System.ComponentModel.DataAnnotations;
using TechNotes.Domain.Enums;

namespace TechNotes.Application.DTOs.Notes;

public sealed class NoteContentRequest
{
    [EnumDataType(typeof(NoteContentType))]
    public NoteContentType Type { get; set; }

    public int SortOrder { get; set; }

    public string? Content { get; set; }

    public string? Data { get; set; }

    public string? Metadata { get; set; }
}
