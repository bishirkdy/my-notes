
using TechNotes.Domain.Common;

namespace TechNotes.Domain.Entities.Identity;
// Stores a renewable sign-in token. It will be used when authentication is added.
public sealed class RefreshToken : BaseEntity
{
    public Guid UserId { get; set; }
    public AppUser User { get; set; } = null!;
    public string Token { get; set; } = string.Empty;
    public DateTime ExpiresAtUtc { get; set; }
    public DateTime? RevokedAtUtc { get; set; }
}
