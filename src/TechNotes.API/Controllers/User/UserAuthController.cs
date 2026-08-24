using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TechNotes.Application.DTOs.Auth;
using TechNotes.Application.Interfaces.Services.Auth;
using TechNotes.Application.Results.Auth;

namespace TechNotes.API.Controllers.User;

// Provides registration, login, refresh, logout, and current-user endpoints for normal and admin users.
[ApiController]
[Route("api/v1/user/auth")]
public sealed class UserAuthController(IAuthService authService) : ControllerBase
{
    private const string RefreshCookieName = "technotes_refresh_token";

    // Creates a normal User account and immediately signs it in.
    [AllowAnonymous]
    [HttpPost("register")]
    [ProducesResponseType<AuthResponse>(StatusCodes.Status201Created)]
    public async Task<ActionResult<AuthResponse>> Register(RegisterRequest request, CancellationToken cancellationToken)
    {
        var result = await authService.RegisterAsync(request, cancellationToken);
        SetRefreshCookie(result.RefreshToken, result.RefreshTokenExpiresAtUtc);

        return StatusCode(StatusCodes.Status201Created, ToResponse(result.Auth));
    }

    // Checks credentials and returns a short-lived access token. The refresh token is written only to a secure cookie.
    [AllowAnonymous]
    [HttpPost("login")]
    [ProducesResponseType<AuthResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<AuthResponse>> Login(LoginRequest request, CancellationToken cancellationToken)
    {
        var result = await authService.LoginAsync(request, cancellationToken);
        if (result is null)
        {
            return Unauthorized(new { message = "Invalid email or password." });
        }

        SetRefreshCookie(result.RefreshToken, result.RefreshTokenExpiresAtUtc);
        return Ok(ToResponse(result.Auth));
    }

    // Replaces an expired access token and rotates the one-time refresh token stored in the cookie.
    [AllowAnonymous]
    [HttpPost("refresh")]
    [ProducesResponseType<AuthResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<AuthResponse>> Refresh(CancellationToken cancellationToken)
    {
        if (!Request.Cookies.TryGetValue(RefreshCookieName, out var refreshToken))
        {
            return Unauthorized(new { message = "Refresh token is missing." });
        }

        var result = await authService.RefreshAsync(refreshToken, cancellationToken);
        if (result is null)
        {
            DeleteRefreshCookie();
            return Unauthorized(new { message = "Refresh token is invalid or expired." });
        }

        SetRefreshCookie(result.RefreshToken, result.RefreshTokenExpiresAtUtc);
        return Ok(ToResponse(result.Auth));
    }

    // Revokes the database token and removes the browser cookie.
    [AllowAnonymous]
    [HttpPost("logout")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Logout(CancellationToken cancellationToken)
    {
        if (Request.Cookies.TryGetValue(RefreshCookieName, out var refreshToken))
        {
            await authService.LogoutAsync(refreshToken, cancellationToken);
        }

        DeleteRefreshCookie();
        return NoContent();
    }

    // Lets an authenticated frontend retrieve the identity encoded in its access token.
    [Authorize]
    [HttpGet("me")]
    public IActionResult Me()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
        return Ok(new
        {
            userId,
            userName = User.Identity?.Name,
            email = User.FindFirstValue(ClaimTypes.Email),
            roles = User.FindAll(ClaimTypes.Role).Select(role => role.Value)
        });
    }

    // HTTP-only prevents JavaScript access; SameSite=Strict helps protect the refresh endpoint from CSRF.
    private void SetRefreshCookie(string refreshToken, DateTime expiresAtUtc) =>
        Response.Cookies.Append(RefreshCookieName, refreshToken, new CookieOptions
        {
            HttpOnly = true,
            Secure = Request.IsHttps,
            SameSite = SameSiteMode.Strict,
            Expires = new DateTimeOffset(expiresAtUtc),
            Path = "/api/user/auth"
        });

    private void DeleteRefreshCookie() =>
        Response.Cookies.Delete(RefreshCookieName, new CookieOptions { Path = "/api/user/auth" });

    private static AuthResponse ToResponse(AuthResult result) =>
        new(result.AccessToken, result.AccessTokenExpiresAtUtc, result.UserId, result.UserName, result.Email, result.Roles);
}
