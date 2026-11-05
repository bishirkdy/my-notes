using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TechNotes.API.Authorization;
using TechNotes.Application.DTOs.Auth;
using TechNotes.Application.Interfaces.Services.Auth;
using TechNotes.Application.Results.Auth;

namespace TechNotes.API.Controllers.Auth;

[ApiController]
[Route("api/v1/auth")]
public sealed class AuthController(
    IAuthService authService,
    IAntiforgery antiforgery,
    IWebHostEnvironment environment) : ControllerBase
{
    [AllowAnonymous]
    [IgnoreAntiforgeryToken]
    [HttpPost("register")]
    [ProducesResponseType<AuthResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<AuthResponse>> Register(RegisterRequest request, CancellationToken cancellationToken)
    {
        var result = await authService.RegisterAsync(request, cancellationToken);
        SetAuthCookies(result);

        return StatusCode(
            StatusCodes.Status201Created,
            ToResponse(result.Auth, CreateCsrfToken(result.Auth)));
    }

    [AllowAnonymous]
    [IgnoreAntiforgeryToken]
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

        SetAuthCookies(result);
        return Ok(ToResponse(result.Auth, CreateCsrfToken(result.Auth)));
    }

    [AllowAnonymous]
    [IgnoreAntiforgeryToken]
    [HttpPost("refresh")]
    [ProducesResponseType<AuthResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<AuthResponse>> Refresh(CancellationToken cancellationToken)
    {
        if (!Request.Cookies.TryGetValue(AuthCookieNames.RefreshToken, out var refreshToken))
        {
            DeleteAuthCookies();
            return Unauthorized(new { message = "Refresh token is missing." });
        }

        var result = await authService.RefreshAsync(refreshToken, cancellationToken);
        if (result is null)
        {
            DeleteAuthCookies();
            return Unauthorized(new { message = "Refresh token is invalid or expired." });
        }

        SetAuthCookies(result);
        return Ok(ToResponse(result.Auth, CreateCsrfToken(result.Auth)));
    }

    [Authorize]
    [HttpPost("logout")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Logout(CancellationToken cancellationToken)
    {
        if (Request.Cookies.TryGetValue(AuthCookieNames.RefreshToken, out var refreshToken))
        {
            await authService.LogoutAsync(refreshToken, cancellationToken);
        }

        DeleteAuthCookies();
        return NoContent();
    }

    [Authorize]
    [HttpGet("me")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public IActionResult Me()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
        var csrfToken = antiforgery.GetAndStoreTokens(HttpContext).RequestToken ?? string.Empty;

        return Ok(new
        {
            userId,
            userName = User.Identity?.Name,
            email = User.FindFirstValue(ClaimTypes.Email) ?? User.FindFirstValue("email"),
            roles = User.FindAll(ClaimTypes.Role).Select(role => role.Value),
            accessTokenExpiresAtUtc = GetAccessTokenExpiresAtUtc(),
            csrfToken
        });
    }

    private void SetAuthCookies(RefreshResult result)
    {
        Response.Cookies.Append(
            AuthCookieNames.AccessToken,
            result.Auth.AccessToken,
            CreateCookieOptions(AuthCookieNames.AccessTokenPath, result.Auth.AccessTokenExpiresAtUtc));

        Response.Cookies.Append(
            AuthCookieNames.RefreshToken,
            result.RefreshToken,
            CreateCookieOptions(AuthCookieNames.RefreshTokenPath, result.RefreshTokenExpiresAtUtc));
    }

    private string CreateCsrfToken(AuthResult result)
    {
        var originalUser = HttpContext.User;
        var token = new JwtSecurityTokenHandler().ReadJwtToken(result.AccessToken);
        var issuer = string.IsNullOrWhiteSpace(token.Issuer)
            ? ClaimsIdentity.DefaultIssuer
            : token.Issuer;
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, result.UserId.ToString(), ClaimValueTypes.String, issuer),
            new(ClaimTypes.Name, result.UserName, ClaimValueTypes.String, issuer),
            new(ClaimTypes.Email, result.Email, ClaimValueTypes.String, issuer)
        };
        claims.AddRange(result.Roles.Select(
            role => new Claim(ClaimTypes.Role, role, ClaimValueTypes.String, issuer)));

        HttpContext.User = new ClaimsPrincipal(new ClaimsIdentity(claims, "CookieAuth"));

        try
        {
            return antiforgery.GetAndStoreTokens(HttpContext).RequestToken ?? string.Empty;
        }
        finally
        {
            HttpContext.User = originalUser;
        }
    }

    private DateTime? GetAccessTokenExpiresAtUtc()
    {
        if (!Request.Cookies.TryGetValue(AuthCookieNames.AccessToken, out var accessToken) ||
            string.IsNullOrWhiteSpace(accessToken))
        {
            return null;
        }

        try
        {
            return new JwtSecurityTokenHandler().ReadJwtToken(accessToken).ValidTo;
        }
        catch
        {
            return null;
        }
    }

    private CookieOptions CreateCookieOptions(string path, DateTime? expiresAtUtc = null)
    {
        var secure = !environment.IsDevelopment();

        return new CookieOptions
        {
            HttpOnly = true,
            Secure = secure,
            SameSite = secure ? SameSiteMode.None : SameSiteMode.Lax,
            Path = path,
            Expires = expiresAtUtc is null ? null : ToUtcOffset(expiresAtUtc.Value),
            IsEssential = true
        };
    }

    private void DeleteAuthCookies()
    {
        Response.Cookies.Delete(
            AuthCookieNames.AccessToken,
            CreateCookieOptions(AuthCookieNames.AccessTokenPath));

        Response.Cookies.Delete(
            AuthCookieNames.RefreshToken,
            CreateCookieOptions(AuthCookieNames.RefreshTokenPath));

        Response.Cookies.Delete(
            AuthCookieNames.Csrf,
            CreateCookieOptions(AuthCookieNames.CsrfPath));
    }

    private static DateTimeOffset ToUtcOffset(DateTime value) =>
        new(value.Kind == DateTimeKind.Utc
            ? value
            : DateTime.SpecifyKind(value, DateTimeKind.Utc));

    private static AuthResponse ToResponse(AuthResult result, string csrfToken) =>
        new(
            result.AccessTokenExpiresAtUtc,
            csrfToken,
            result.UserId,
            result.UserName,
            result.Email,
            result.Roles);
}
