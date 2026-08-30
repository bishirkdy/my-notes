
using TechNotes.Domain.Common;
// Represents a heading or subheading and supports unlimited nesting.
namespace TechNotes.Domain.Entities.Notes
{
    public class NoteSection : BaseEntity
    {
        public Guid NoteId { get; set; }
        public Note Note { get; set; } = null!;
        public Guid? ParentSectionId { get; set; }
        public NoteSection? ParentSection { get; set; }
        public ICollection<NoteSection> Children { get; set; } = [];
        public string Title { get; set; } = string.Empty;
        public int SortOrder { get; set; }
        public ICollection<NoteContent> Contents { get; set; } = [];
    }
}
