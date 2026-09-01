using System.ComponentModel.DataAnnotations;
using TechNotes.Domain.Enums;

namespace TechNotes.Application.DTOs.Notes
{
    // Defines the data required to create a new note.
    public sealed class CreateNoteRequest
    {
        [Required]
        [StringLength(200, MinimumLength = 1)]
        public string Title { get; set; } = string.Empty;

        // Public allows everyone to read the note by default.
        public NoteVisibility Visibility { get; set; } = NoteVisibility.Public;

        // Allows the note to be organized into an optional folder.
        public Guid? FolderId { get; set; }

        // Allows the note to be classified under an optional category.
        public Guid? CategoryId { get; set; }

        // Contains the complete heading and content structure of the note.
        public List<NoteSectionRequest> Sections { get; set; } = [];
    }
}
