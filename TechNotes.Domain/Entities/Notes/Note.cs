using TechNotes.Domain.Common;
using TechNotes.Domain.Entities.Identity;
using TechNotes.Domain.Enums;

namespace TechNotes.Domain.Entities.Notes;

// The main content record. A note belongs to one owner and can be organized, shared, and versioned.
public sealed class Note : BaseEntity
{
    public Guid OwnerId { get; set; }
    public AppUser Owner { get; set; } = null!;
    public Guid? FolderId { get; set; }
    public Folder? Folder { get; set; }
    public Guid? CategoryId { get; set; }
    public Category? Category { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public NoteStatus Status { get; set; } = NoteStatus.Draft;
    public NoteVisibility Visibility { get; set; } = NoteVisibility.Private;
    public bool IsPinned { get; set; }
    public DateTime? PublishedAtUtc { get; set; }

    // Navigation properties for the note's labels, collaboration, activity, and history.
    public ICollection<NoteTag> NoteTags { get; set; } = new List<NoteTag>();
    public ICollection<NoteShare> Shares { get; set; } = new List<NoteShare>();
    public ICollection<Comment> Comments { get; set; } = new List<Comment>();
    public ICollection<Attachment> Attachments { get; set; } = new List<Attachment>();
    public ICollection<Favorite> Favorites { get; set; } = new List<Favorite>();
    public ICollection<NoteVersion> Versions { get; set; } = new List<NoteVersion>();
}
