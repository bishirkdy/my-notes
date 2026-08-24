using TechNotes.Domain.Common;
using TechNotes.Domain.Entities.Identity;

namespace TechNotes.Domain.Entities.Notes;

// Saves a historical snapshot of a note so users can view or restore earlier content.
public sealed class NoteVersion : BaseEntity
{
    public Guid NoteId { get; set; }
    public Note Note { get; set; } = null!;
    public Guid CreatedById { get; set; }
    public AppUser CreatedBy { get; set; } = null!;
    public int VersionNumber { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
}
