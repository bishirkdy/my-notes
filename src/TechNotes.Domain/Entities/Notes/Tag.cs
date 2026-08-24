using TechNotes.Domain.Common;
using TechNotes.Domain.Entities.Identity;

namespace TechNotes.Domain.Entities.Notes;

// A reusable user-owned label that can be attached to many notes.
public sealed class Tag : BaseEntity
{
    public Guid OwnerId { get; set; }
    public AppUser Owner { get; set; } = null!;
    public string Name { get; set; } = string.Empty;

    public ICollection<NoteTag> NoteTags { get; set; } = new List<NoteTag>();
}
