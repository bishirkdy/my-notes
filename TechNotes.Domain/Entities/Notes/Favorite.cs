using TechNotes.Domain.Common;
using TechNotes.Domain.Entities.Identity;

namespace TechNotes.Domain.Entities.Notes;

// Marks a note as saved by a user for quick access.
public sealed class Favorite : BaseEntity
{
    public Guid UserId { get; set; }
    public AppUser User { get; set; } = null!;
    public Guid NoteId { get; set; }
    public Note Note { get; set; } = null!;
}
