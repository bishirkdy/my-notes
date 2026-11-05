namespace TechNotes.API.Authorization;

public static class AuthCookieNames
{
    public const string AccessToken = "technotes_access_token";
    public const string RefreshToken = "technotes_refresh_token";
    public const string Csrf = "technotes_csrf";

    public const string AccessTokenPath = "/api/v1";
    public const string RefreshTokenPath = "/api/v1/auth";
    public const string CsrfPath = "/api/v1";
}
