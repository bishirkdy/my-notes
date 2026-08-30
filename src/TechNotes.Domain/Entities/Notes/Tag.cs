using TechNotes.Domain.Common;
using TechNotes.Domain.Entities.Identity;

namespace TechNotes.Domain.Entities.Notes;

// Represents a reusable keyword that can be attached to many notes.
public class Tag : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public ICollection<NoteTag> NoteTags { get; set; } = [];
}
