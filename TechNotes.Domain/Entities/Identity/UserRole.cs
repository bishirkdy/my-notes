namespace TechNotes.Domain.Entities.Identity;
// Join table that allows one user to have one or more roles.
public sealed class UserRole
{
    public Guid UserId { get; set; }
    public AppUser User { get; set; } = null!;
    public Guid RoleId { get; set; }
    public Role Role { get; set; } = null!;
}
