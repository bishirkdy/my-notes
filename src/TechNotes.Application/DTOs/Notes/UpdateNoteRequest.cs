using System.ComponentModel.DataAnnotations;
using TechNotes.Domain.Enums;

namespace TechNotes.Application.DTOs.Notes;

public sealed class UpdateNoteRequest
{
    [Required]
    [StringLength(200, MinimumLength = 1)]
    public string Title { get; set; } = string.Empty;

    [EnumDataType(typeof(NoteVisibility))]
    public NoteVisibility? Visibility { get; set; }

    [EnumDataType(typeof(NoteStatus))]
    public NoteStatus? Status { get; set; }

    public Guid? FolderId { get; set; }

    public Guid? CategoryId { get; set; }

    public List<string> Tags { get; set; } = [];

    public List<NoteSectionRequest> Sections { get; set; } = [];
}
