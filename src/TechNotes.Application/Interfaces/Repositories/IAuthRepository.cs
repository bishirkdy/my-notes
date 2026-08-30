using TechNotes.Domain.Entities.Identity;

namespace TechNotes.Application.Interfaces.Repositories;

public interface IAuthRepository
{
    Task<bool> UserExistsAsync(string email,string userName,CancellationToken cancellationToken);

    Task<AppUser?> GetUserWithRolesAsync(string email, CancellationToken cancellationToken);

    Task<Role?> GetRoleByNameAsync(string roleName, CancellationToken cancellationToken);

    Task AddUserAsync(AppUser user,CancellationToken cancellationToken);

    Task<RefreshToken?> GetRefreshTokenWithUserAsync(string tokenHash, CancellationToken cancellationToken);

    Task<RefreshToken?> GetRefreshTokenAsync(string tokenHash, CancellationToken cancellationToken);

    Task AddRefreshTokenAsync(RefreshToken refreshToken, CancellationToken cancellationToken);

    Task SaveChangesAsync(CancellationToken cancellationToken);
}