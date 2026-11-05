namespace TechNotes.Application.Results.Auth;

public sealed record AuthResult(
    string AccessToken,
    DateTime AccessTokenExpiresAtUtc,
    Guid UserId,
    string UserName,
    string Email,
    IReadOnlyCollection<string> Roles);
