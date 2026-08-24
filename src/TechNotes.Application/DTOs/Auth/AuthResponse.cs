namespace TechNotes.Application.DTOs.Auth;

// Safe authentication data returned to the client. The refresh token is kept only in an HTTP-only cookie.
public sealed record AuthResponse(
    string AccessToken,
    DateTime AccessTokenExpiresAtUtc,
    Guid UserId,
    string UserName,
    string Email,
    IReadOnlyCollection<string> Roles);
