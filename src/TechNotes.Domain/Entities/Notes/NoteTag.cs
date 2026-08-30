using TechNotes.Domain.Common;

namespace TechNotes.Domain.Entities.Notes;

// Connects notes and reusable tags through a many-to-many relationship.
public class NoteTag : BaseEntity
{
    public Guid NoteId { get; set; }
    public Note Note { get; set; } = null!;
    public Guid TagId { get; set; }
    public Tag Tag { get; set; } = null!;
}