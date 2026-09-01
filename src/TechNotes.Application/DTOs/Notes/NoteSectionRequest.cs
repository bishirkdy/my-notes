using System.ComponentModel.DataAnnotations;

namespace TechNotes.Application.DTOs.Notes
{
    // Represents a heading or nested subheading supplied by the client.
    public sealed class NoteSectionRequest
    {
        [Required]
        [StringLength(300, MinimumLength = 1)]
        public string Title { get; set; } = string.Empty;

        // Controls the order of this heading among its siblings.
        public int SortOrder { get; set; }

        // Allows unlimited nested headings and subheadings.
        public List<NoteSectionRequest> Children { get; set; } = [];

        // Allows this heading to contain any combination of content blocks.
        public List<NoteContentRequest> Contents { get; set; } = [];
    }
}
    