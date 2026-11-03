using TechNotes.Domain.Enums;

namespace TechNotes.Application.DTOs.Notes
{
    // Represents the complete note returned to the API client.
    public sealed class NoteResponse
    {
        public Guid Id { get; set; }

        public string Title { get; set; } = string.Empty;

        public NoteStatus Status { get; set; }

        public NoteVisibility Visibility { get; set; }

        public Guid UserId { get; set; }

        public Guid? FolderId { get; set; }

        public Guid? CategoryId { get; set; }

        public string? CategoryName { get; set; }

        public string? CategoryParentName { get; set; }

        public List<string> CategoryPath { get; set; } = [];

        public List<string> Tags { get; set; } = [];

        public DateTime CreatedAtUtc { get; set; }

        public DateTime? UpdatedAtUtc { get; set; }

        public ReadingProgressSummary? ReadingProgress { get; set; }

        // Returns the complete hierarchical note structure.
        public List<NoteSectionResponse> Sections { get; set; } = [];
    }
}
