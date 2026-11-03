using TechNotes.Domain.Common;
using TechNotes.Domain.Entities.Identity;
using TechNotes.Domain.Enums;

namespace TechNotes.Domain.Entities.Notes;

// The main content record. A note belongs to one owner and can be organized, shared, and versioned.
public sealed class Note : BaseEntity
{
    public Guid UserId { get; set; }
    public AppUser User { get; set; } = null!;
    public string Title { get; set; } = string.Empty;
    public NoteStatus Status { get; set; }
    public NoteVisibility Visibility { get; set; }
    public Guid? FolderId { get; set; }
    public Folder? Folder { get; set; }
    public Guid? CategoryId { get; set; }
    public Category? Category { get; set; }
    public ICollection<NoteSection> Sections { get; set; } = [];
    public ICollection<NoteTag> NoteTags { get; set; } = [];
    public ICollection<NoteShare> Shares { get; set; } = [];
    public ICollection<Comment> Comments { get; set; } = [];
    public ICollection<Attachment> Attachments { get; set; } = [];
    public ICollection<Favorite> Favorites { get; set; } = [];
    public ICollection<ReadingProgress> ReadingProgress { get; set; } = [];
    public ICollection<NoteVersion> Versions { get; set; } = [];
}
