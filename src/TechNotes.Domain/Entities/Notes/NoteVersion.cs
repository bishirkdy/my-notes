using TechNotes.Domain.Common;
using TechNotes.Domain.Entities.Identity;

namespace TechNotes.Domain.Entities.Notes;

// Stores a historical snapshot of a note for version tracking and recovery.
public class NoteVersion : BaseEntity
{
    public Guid NoteId { get; set; }
    public Note Note { get; set; } = null!;
    public int VersionNumber { get; set; }
    public string Title { get; set; } = string.Empty;
    // Stores the structured note content snapshot as JSON.
    public string Content { get; set; } = string.Empty;
    public Guid CreatedByUserId { get; set; }
    public AppUser CreatedByUser { get; set; } = null!;
}
