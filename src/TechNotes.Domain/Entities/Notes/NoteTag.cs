namespace TechNotes.Domain.Entities.Notes;

// Join table that creates the many-to-many relationship between notes and tags.
public sealed class NoteTag
{
    public Guid NoteId { get; set; }
    public Note Note { get; set; } = null!;
    public Guid TagId { get; set; }
    public Tag Tag { get; set; } = null!;
}
