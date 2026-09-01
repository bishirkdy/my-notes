using System.ComponentModel.DataAnnotations;
using TechNotes.Domain.Enums;

namespace TechNotes.Application.DTOs.Notes
{
    // Represents one dynamic content block inside a section.
    public sealed class NoteContentRequest
    {
        // Defines whether this block is a definition, list, table, example, code, etc.
        [Required]
        public NoteContentType Type { get; set; }

        // Controls the order of this content block within the section.
        public int SortOrder { get; set; }

        // Stores normal text-based content.
        public string? Content { get; set; }

        // Stores structured JSON data such as tables, lists, and graphs.
        public string? Data { get; set; }

        // Stores optional rendering metadata such as programming language.
        public string? Metadata { get; set; }
    }
}
