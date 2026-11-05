namespace TechNotes.Application.DTOs.Auth;

public sealed record AuthResponse(
    DateTime AccessTokenExpiresAtUtc,
    string CsrfToken,
    Guid UserId,
    string UserName,
    string Email,
    IReadOnlyCollection<string> Roles);
