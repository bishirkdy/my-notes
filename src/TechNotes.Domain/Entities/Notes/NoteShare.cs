using TechNotes.Domain.Common;
using TechNotes.Domain.Entities.Identity;
using TechNotes.Domain.Enums;

namespace TechNotes.Domain.Entities.Notes;

// Grants another user controlled access to a note.
public class NoteShare : BaseEntity
{
    public Guid NoteId { get; set; }
    public Note Note { get; set; } = null!;
    public Guid UserId { get; set; }
    public AppUser User { get; set; } = null!;
    public SharePermission Permission { get; set; }
}
