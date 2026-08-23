using TechNotes.Domain.Common;
using TechNotes.Domain.Entities.Identity;

namespace TechNotes.Domain.Entities.Notes;

// A message left on a note. A comment can optionally reply to another comment.
public sealed class Comment : BaseEntity
{
    public Guid NoteId { get; set; }
    public Note Note { get; set; } = null!;
    public Guid AuthorId { get; set; }
    public AppUser Author { get; set; } = null!;
    public Guid? ParentCommentId { get; set; }
    public Comment? ParentComment { get; set; }
    public string Content { get; set; } = string.Empty;
    public bool IsDeleted { get; set; }

    public ICollection<Comment> Replies { get; set; } = new List<Comment>();
}
