using TechNotes.Domain.Common;
using TechNotes.Domain.Entities.Identity;
using TechNotes.Domain.Enums;

namespace TechNotes.Domain.Entities.Notes;

// Grants one user a specific level of access to another user's note.
public sealed class NoteShare : BaseEntity
{
    public Guid NoteId { get; set; }
    public Note Note { get; set; } = null!;
    public Guid RecipientId { get; set; }
    public AppUser Recipient { get; set; } = null!;
    public SharePermission Permission { get; set; } = SharePermission.View;
}
