
using TechNotes.Domain.Common;

namespace TechNotes.Domain.Entities.Identity;
// A permission group, for example Admin or User.
public sealed class Role : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }

    public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
}
