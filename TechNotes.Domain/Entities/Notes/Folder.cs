using TechNotes.Domain.Common;
using TechNotes.Domain.Entities.Identity;

namespace TechNotes.Domain.Entities.Notes;

// A user-owned folder that can contain notes and nested folders.
public sealed class Folder : BaseEntity
{
    public Guid OwnerId { get; set; }
    public AppUser Owner { get; set; } = null!;
    public Guid? ParentFolderId { get; set; }
    public Folder? ParentFolder { get; set; }
    public string Name { get; set; } = string.Empty;

    public ICollection<Folder> ChildFolders { get; set; } = new List<Folder>();
    public ICollection<Note> Notes { get; set; } = new List<Note>();
}
