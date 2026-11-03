using TechNotes.Domain.Common;
using TechNotes.Domain.Enums;
using TechNotes.Domain.Entities.Notes;
using TechNotes.Domain.Entities.Notifications;


namespace TechNotes.Domain.Entities.Identity;
// Represents one person who can sign in, own notes, and receive notifications.
public sealed class AppUser : BaseEntity
{
    public string UserName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string? DisplayName { get; set; }
    public string? ProfileImageUrl { get; set; }
    public AccountStatus Status { get; set; } = AccountStatus.Active;

    // Navigation properties connect this user to related records in the database.
    public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
    public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
    public ICollection<Folder> Folders { get; set; } = new List<Folder>();
    public ICollection<Category> Categories { get; set; } = new List<Category>();
    public ICollection<Tag> Tags { get; set; } = new List<Tag>();
    public ICollection<Note> Notes { get; set; } = new List<Note>();
    public ICollection<Comment> Comments { get; set; } = new List<Comment>();
    public ICollection<Favorite> Favorites { get; set; } = new List<Favorite>();
    public ICollection<ReadingProgress> ReadingProgress { get; set; } = new List<ReadingProgress>();
    public ICollection<Notification> Notifications { get; set; } = new List<Notification>();
}
