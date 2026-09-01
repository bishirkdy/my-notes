
namespace TechNotes.Application.DTOs.Notes
{
    // Represents a heading or subheading returned to the client.
    public sealed class NoteSectionResponse
    {
        public Guid Id { get; set; }

        public string Title { get; set; } = string.Empty;

        public int SortOrder { get; set; }

        // Contains nested headings and subheadings.
        public List<NoteSectionResponse> Children { get; set; } = [];

        // Contains all content blocks belonging to this section.
        public List<NoteContentResponse> Contents { get; set; } = [];
    }
}
