

using TechNotes.Domain.Enums;
// Represents one flexible content block inside a note section.
namespace TechNotes.Domain.Entities.Notes
{
    public class NoteContent
    {
        public Guid SectionId { get; set; }
        public NoteSection Section { get; set; } = null!;
        public NoteContentType Type { get; set; }
        public int SortOrder { get; set; }
        // Stores simple text-based content.
        public string? Content { get; set; }
        // Stores structured JSON for tables, lists, graphs, etc.
        public string? Data { get; set; }
        // Stores optional rendering information such as language or display settings.
        public string? Metadata { get; set; }
    }
}
