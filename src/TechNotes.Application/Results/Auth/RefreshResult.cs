namespace TechNotes.Application.Results.Auth;

public sealed record RefreshResult(
    AuthResult Auth, 
    string RefreshToken, 
    DateTime RefreshTokenExpiresAtUtc
);
