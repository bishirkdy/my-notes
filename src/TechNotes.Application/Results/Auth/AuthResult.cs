namespace TechNotes.Application.Results.Auth;

// Information returned after a successful login, registration, or refresh request.
public sealed record AuthResult(
    string AccessToken,
    DateTime AccessTokenExpiresAtUtc,
    Guid UserId,
    string UserName,
    string Email,
    IReadOnlyCollection<string> Roles);
