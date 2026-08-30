using TechNotes.Domain.Common;
using TechNotes.Domain.Entities.Identity;

namespace TechNotes.Domain.Entities.Notes;

// Represents a user's favorite relationship with a note.
public class Favorite : BaseEntity
{
    public Guid NoteId { get; set; }
    public Note Note { get; set; } = null!;
    public Guid UserId { get; set; }
    public AppUser User { get; set; } = null!;
}
