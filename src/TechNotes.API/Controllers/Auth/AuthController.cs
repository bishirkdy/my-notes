using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TechNotes.Application.DTOs.Auth;
using TechNotes.Application.Interfaces.Services.Auth;
using TechNotes.Application.Results.Auth;

namespace TechNotes.API.Controllers.Auth;

[ApiController]
[Route("api/v1/auth")]
public sealed class AuthController(IAuthService authService) : ControllerBase
{
    private const string RefreshCookieName = "technotes_refresh_token";
    private const string RefreshCookiePath = "/api/v1/auth";

    [AllowAnonymous]
    [HttpPost("register")]
    [ProducesResponseType<AuthResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<AuthResponse>> Register(RegisterRequest request, CancellationToken cancellationToken)
    {
        var result = await authService.RegisterAsync(request, cancellationToken);
        SetRefreshCookie(result.RefreshToken, result.RefreshTokenExpiresAtUtc);

        return StatusCode(StatusCodes.Status201Created, ToResponse(result.Auth));
    }

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

    [Authorize]
    [HttpGet("me")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public IActionResult Me()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
        return Ok(new
        {
            userId,
            userName = User.Identity?.Name,
            email = User.FindFirstValue(ClaimTypes.Email) ?? User.FindFirstValue("email"),
            roles = User.FindAll(ClaimTypes.Role).Select(role => role.Value)
        });
    }

    // The refresh token must stay out of JavaScript, and the path must match the auth routes.
    private void SetRefreshCookie(string refreshToken, DateTime expiresAtUtc) =>
        Response.Cookies.Append(RefreshCookieName, refreshToken, CreateRefreshCookieOptions(
            expires: new DateTimeOffset(expiresAtUtc)));

    private void DeleteRefreshCookie() =>
        Response.Cookies.Delete(RefreshCookieName, CreateRefreshCookieOptions());

    private CookieOptions CreateRefreshCookieOptions(DateTimeOffset? expires = null) => new()
    {
        HttpOnly = true,
        Secure = Request.IsHttps,
        SameSite = SameSiteMode.Strict,
        Path = RefreshCookiePath,
        Expires = expires
    };

    private static AuthResponse ToResponse(AuthResult result) =>
        new(
            result.AccessToken,
            result.AccessTokenExpiresAtUtc,
            result.UserId,
            result.UserName,
            result.Email,
            result.Roles);
}
