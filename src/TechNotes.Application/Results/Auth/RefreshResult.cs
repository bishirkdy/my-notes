namespace TechNotes.Application.Results.Auth;

// Contains the new access-token response plus the new opaque refresh token for the secure cookie.
public sealed record RefreshResult(
    AuthResult Auth,
    string RefreshToken,
    DateTime RefreshTokenExpiresAtUtc);
