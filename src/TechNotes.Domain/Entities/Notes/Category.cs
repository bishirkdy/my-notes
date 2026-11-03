using TechNotes.Domain.Common;
using TechNotes.Domain.Entities.Identity;

namespace TechNotes.Domain.Entities.Notes;

// Classifies notes into reusable subjects such as C#, JavaScript, or SQL.
public class Category : BaseEntity
{
    public Guid UserId { get; set; }
    public AppUser User { get; set; } = null!;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public Guid? ParentCategoryId { get; set; }
    public Category? ParentCategory { get; set; }
    public int SortOrder { get; set; }
    public ICollection<Category> Children { get; set; } = [];
    public ICollection<Note> Notes { get; set; } = [];
}
