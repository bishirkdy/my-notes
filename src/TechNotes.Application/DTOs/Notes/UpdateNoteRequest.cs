

using System.ComponentModel.DataAnnotations;
using TechNotes.Domain.Enums;

namespace TechNotes.Application.DTOs.Notes
{
    // Defines the editable data for an existing note.
    public sealed class UpdateNoteRequest
    {
        [Required]
        [StringLength(200, MinimumLength = 1)]
        public string Title { get; set; } = string.Empty;

        // Controls whether the note is private, shared, or public.
        public NoteVisibility Visibility { get; set; }

        // Updates the folder organization.
        public Guid? FolderId { get; set; }

        // Updates the note classification.
        public Guid? CategoryId { get; set; }

        // Replaces or updates the structured sections of the note.
        public List<NoteSectionRequest> Sections { get; set; } = [];
    }
}
