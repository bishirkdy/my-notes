using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;
using TechNotes.Application.DTOs.Auth;
using TechNotes.Application.Interfaces.Repositories;
using TechNotes.Application.Interfaces.Services.Auth;
using TechNotes.Application.Results.Auth;
using TechNotes.Application.Settings.Auth;
using TechNotes.Domain.Constants;
using TechNotes.Domain.Entities.Identity;
using TechNotes.Domain.Enums;

namespace TechNotes.Infrastructure.Services.Auth;

public sealed class AuthService(IAuthRepository authRepository, IPasswordHasher passwordHasher, IJwtTokenService jwtTokenService, IOptions<JwtOptions> jwtOptions) : IAuthService
{
    private readonly JwtOptions _jwtOptions = jwtOptions.Value;

    public async Task<RefreshResult> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken)
    {
        var email = request.Email.Trim().ToLowerInvariant();
        var userName = request.UserName.Trim();

        var alreadyExists = await authRepository.UserExistsAsync(email,userName,cancellationToken);

        if (alreadyExists)
        {
            throw new ArgumentException("An account with that email address or username already exists.");
        }

        var userRole = await authRepository.GetRoleByNameAsync(RoleNames.User,cancellationToken);

        if (userRole is null)
        {
            throw new InvalidOperationException("Default User role has not been seeded.");
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

        await authRepository.AddUserAsync(user,cancellationToken);
        await authRepository.SaveChangesAsync(cancellationToken);

        return await IssueTokensAsync(user, [RoleNames.User], cancellationToken);
    }

    public async Task<RefreshResult?> LoginAsync(LoginRequest request, CancellationToken cancellationToken)
    {
        var email = request.Email.Trim().ToLowerInvariant();

        var user = await authRepository.GetUserWithRolesAsync(email,cancellationToken);

        if (user is null || user.Status != AccountStatus.Active)
        {
            return null;
        }

        var isValid = passwordHasher.VerifyPassword(request.Password, user.PasswordHash);

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

        var storedToken =
            await authRepository.GetRefreshTokenWithUserAsync(
                tokenHash,
                cancellationToken);

        if (storedToken is null ||
            storedToken.RevokedAtUtc is not null ||
            storedToken.ExpiresAtUtc <= DateTime.UtcNow ||
            storedToken.User.Status != AccountStatus.Active)
        {
            return null;
        }

        // Revoke old refresh token.
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

        var storedToken = await authRepository.GetRefreshTokenAsync(
            tokenHash,
            cancellationToken);

        if (storedToken is not null &&
            storedToken.RevokedAtUtc is null)
        {
            storedToken.RevokedAtUtc = DateTime.UtcNow;

            await authRepository.SaveChangesAsync(
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

        var refreshTokenEntity = new RefreshToken
        {
            UserId = user.Id,
            Token = HashToken(refreshToken),
            ExpiresAtUtc = refreshExpiresAtUtc
        };

        await authRepository.AddRefreshTokenAsync(
            refreshTokenEntity,
            cancellationToken);

        await authRepository.SaveChangesAsync(
            cancellationToken);

        return new RefreshResult(
            jwtTokenService.CreateAccessToken(user, roles),
            refreshToken,
            refreshExpiresAtUtc);
    }

    private static string HashToken(string token)
    {
        var bytes = Encoding.UTF8.GetBytes(token);

        var hash = SHA256.HashData(bytes);

        return Convert.ToBase64String(hash);
    }
}