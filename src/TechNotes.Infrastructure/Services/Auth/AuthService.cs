using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using TechNotes.Application.DTOs.Auth;
using TechNotes.Application.Interfaces.Services.Auth;
using TechNotes.Application.Results.Auth;
using TechNotes.Application.Settings.Auth;
using TechNotes.Domain.Constants;
using TechNotes.Domain.Entities.Identity;
using TechNotes.Domain.Enums;
using TechNotes.Persistence.Data;

namespace TechNotes.Infrastructure.Services.Auth;

// Handles registration, login, refresh-token rotation, and logout.
public sealed class AuthService(
    AppDbContext dbContext,
    IPasswordHasher passwordHasher,
    IJwtTokenService jwtTokenService,
    IOptions<JwtOptions> jwtOptions) : IAuthService
{
    private readonly JwtOptions _jwtOptions = jwtOptions.Value;

    public async Task<RefreshResult> RegisterAsync(
        RegisterRequest request,
        CancellationToken cancellationToken)
    {
        var email = request.Email.Trim().ToLowerInvariant();
        var userName = request.UserName.Trim();

        var alreadyExists = await dbContext.Users.AnyAsync(
            user =>
                user.Email == email ||
                user.UserName == userName,
            cancellationToken);

        if (alreadyExists)
        {
            throw new ArgumentException(
                "An account with that email address or username already exists.");
        }

        var userRole = await dbContext.Roles.SingleOrDefaultAsync(
            role => role.Name == RoleNames.User,
            cancellationToken);

        if (userRole is null)
        {
            throw new InvalidOperationException(
                "Default User role has not been seeded.");
        }

        var user = new AppUser
        {
            Email = email,
            UserName = userName,
            PasswordHash = passwordHasher.HashPassword(request.Password)
        };

        user.UserRoles.Add(new UserRole
        {
            Role = userRole
        });

        dbContext.Users.Add(user);

        await dbContext.SaveChangesAsync(cancellationToken);

        return await IssueTokensAsync(
            user,
            [RoleNames.User],
            cancellationToken);
    }

    public async Task<RefreshResult?> LoginAsync(
        LoginRequest request,
        CancellationToken cancellationToken)
    {
        var email = request.Email.Trim().ToLowerInvariant();

        var user = await LoadUserWithRolesAsync(
            email,
            cancellationToken);

        if (user is null ||
            user.Status != AccountStatus.Active)
        {
            return null;
        }

        var isValid = passwordHasher.VerifyPassword(
            request.Password,
            user.PasswordHash);

        if (!isValid)
        {
            return null;
        }

        var roles = user.UserRoles
            .Select(userRole => userRole.Role.Name)
            .ToArray();

        return await IssueTokensAsync(
            user,
            roles,
            cancellationToken);
    }

    public async Task<RefreshResult?> RefreshAsync(
        string refreshToken,
        CancellationToken cancellationToken)
    {
        var tokenHash = HashToken(refreshToken);

        var storedToken = await dbContext.RefreshTokens
            .Include(token => token.User)
            .ThenInclude(user => user.UserRoles)
            .ThenInclude(userRole => userRole.Role)
            .SingleOrDefaultAsync(
                token => token.Token == tokenHash,
                cancellationToken);

        if (storedToken is null ||
            storedToken.RevokedAtUtc is not null ||
            storedToken.ExpiresAtUtc <= DateTime.UtcNow ||
            storedToken.User.Status != AccountStatus.Active)
        {
            return null;
        }

        // Invalidate the old refresh token.
        storedToken.RevokedAtUtc = DateTime.UtcNow;

        var roles = storedToken.User.UserRoles
            .Select(userRole => userRole.Role.Name)
            .ToArray();

        return await IssueTokensAsync(
            storedToken.User,
            roles,
            cancellationToken);
    }

    public async Task LogoutAsync(
        string refreshToken,
        CancellationToken cancellationToken)
    {
        var tokenHash = HashToken(refreshToken);

        var storedToken = await dbContext.RefreshTokens
            .SingleOrDefaultAsync(
                token => token.Token == tokenHash,
                cancellationToken);

        if (storedToken is not null &&
            storedToken.RevokedAtUtc is null)
        {
            storedToken.RevokedAtUtc = DateTime.UtcNow;

            await dbContext.SaveChangesAsync(
                cancellationToken);
        }
    }

    private async Task<RefreshResult> IssueTokensAsync(
        AppUser user,
        IReadOnlyCollection<string> roles,
        CancellationToken cancellationToken)
    {
        var refreshToken = Convert.ToBase64String(
            RandomNumberGenerator.GetBytes(64));

        var refreshExpiresAtUtc = DateTime.UtcNow.AddDays(
            _jwtOptions.RefreshTokenDays);

        dbContext.RefreshTokens.Add(new RefreshToken
        {
            UserId = user.Id,
            Token = HashToken(refreshToken),
            ExpiresAtUtc = refreshExpiresAtUtc
        });

        await dbContext.SaveChangesAsync(
            cancellationToken);

        return new RefreshResult(
            jwtTokenService.CreateAccessToken(user, roles),
            refreshToken,
            refreshExpiresAtUtc);
    }

    private Task<AppUser?> LoadUserWithRolesAsync(
        string email,
        CancellationToken cancellationToken)
    {
        return dbContext.Users
            .Include(user => user.UserRoles)
            .ThenInclude(userRole => userRole.Role)
            .SingleOrDefaultAsync(
                user => user.Email == email,
                cancellationToken);
    }

    private static string HashToken(string token)
    {
        var bytes = Encoding.UTF8.GetBytes(token);

        var hash = SHA256.HashData(bytes);

        return Convert.ToBase64String(hash);
    }
}