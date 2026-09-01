using TechNotes.Domain.Enums;

namespace TechNotes.Application.DTOs.Notes
{
    // Represents a dynamic content block returned to the client.
    public sealed class NoteContentResponse
    {
        public Guid Id { get; set; }

        public NoteContentType Type { get; set; }

        public int SortOrder { get; set; }

        public string? Content { get; set; }

        // Returns structured data such as tables, lists, and graphs.
        public string? Data { get; set; }

        // Returns rendering information such as code language.
        public string? Metadata { get; set; }
    }
}
