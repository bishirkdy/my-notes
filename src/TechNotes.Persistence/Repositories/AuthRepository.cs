using Microsoft.EntityFrameworkCore;
using TechNotes.Application.Interfaces.Repositories;
using TechNotes.Domain.Entities.Identity;
using TechNotes.Persistence.Data;

namespace TechNotes.Persistence.Repositories.Auth;

public sealed class AuthRepository(AppDbContext dbContext) : IAuthRepository
{
    public Task<bool> UserExistsAsync(string email,string userName,CancellationToken cancellationToken)
    {
        return dbContext.Users.AnyAsync(user => user.Email == email ||user.UserName == userName,cancellationToken);
    }

    public Task<AppUser?> GetUserWithRolesAsync(string email, CancellationToken cancellationToken)
    {
        return dbContext.Users
            .Include(user => user.UserRoles)
            .ThenInclude(userRole => userRole.Role)
            .SingleOrDefaultAsync(user => user.Email == email, cancellationToken);
    }

    public Task<Role?> GetRoleByNameAsync(string roleName, CancellationToken cancellationToken)
    {
        return dbContext.Roles.SingleOrDefaultAsync(role => role.Name == roleName, cancellationToken);
    }

    public async Task AddUserAsync(AppUser user, CancellationToken cancellationToken)
    {
        await dbContext.Users.AddAsync(user,cancellationToken);
    }

    public Task<RefreshToken?> GetRefreshTokenWithUserAsync(string tokenHash,CancellationToken cancellationToken)
    {
        return dbContext.RefreshTokens
            .Include(token => token.User)
            .ThenInclude(user => user.UserRoles)
            .ThenInclude(userRole => userRole.Role)
            .SingleOrDefaultAsync(token => token.Token == tokenHash, cancellationToken);
    }

    public Task<RefreshToken?> GetRefreshTokenAsync(string tokenHash, CancellationToken cancellationToken)
    {
        return dbContext.RefreshTokens.SingleOrDefaultAsync(token => token.Token == tokenHash, cancellationToken);
    }

    public async Task AddRefreshTokenAsync(RefreshToken refreshToken, CancellationToken cancellationToken)
    {
        await dbContext.RefreshTokens.AddAsync(refreshToken, cancellationToken);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        return dbContext.SaveChangesAsync(cancellationToken);
    }
}