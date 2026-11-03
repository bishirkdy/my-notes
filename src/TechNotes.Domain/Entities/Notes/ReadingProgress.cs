using TechNotes.Domain.Common;
using TechNotes.Domain.Entities.Identity;

namespace TechNotes.Domain.Entities.Notes;

public sealed class ReadingProgress : BaseEntity
{
    public Guid UserId { get; set; }
    public AppUser User { get; set; } = null!;
    public Guid NoteId { get; set; }
    public Note Note { get; set; } = null!;
    public int Percent { get; set; }
    public DateTime LastReadAtUtc { get; set; } = DateTime.UtcNow;
}
