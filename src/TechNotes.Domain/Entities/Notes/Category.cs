using TechNotes.Domain.Common;
using TechNotes.Domain.Entities.Identity;

namespace TechNotes.Domain.Entities.Notes;

// A user-owned category for grouping related notes.
public sealed class Category : BaseEntity
{
    public Guid OwnerId { get; set; }
    public AppUser Owner { get; set; } = null!;
    public string Name { get; set; } = string.Empty;
    public string? Color { get; set; }

    public ICollection<Note> Notes { get; set; } = new List<Note>();
}
