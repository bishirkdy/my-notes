using TechNotes.Domain.Common;
using TechNotes.Domain.Entities.Identity;
using TechNotes.Domain.Entities.Notes;
using TechNotes.Domain.Enums;

namespace TechNotes.Domain.Entities.Notifications;

// A message shown to a user when sharing, comments, mentions, or system events occur.
public sealed class Notification : BaseEntity
{
    public Guid UserId { get; set; }
    public AppUser User { get; set; } = null!;
    public Guid? NoteId { get; set; }
    public Note? Note { get; set; }
    public NotificationType Type { get; set; }
    public string Message { get; set; } = string.Empty;
    public bool IsRead { get; set; }
    public DateTime? ReadAtUtc { get; set; }
}
