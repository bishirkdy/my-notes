using System.Security.Claims;
using System.Text;
using System.IdentityModel.Tokens.Jwt;
using TechNotes.Application.Results.Auth;
using TechNotes.Application.Settings.Auth;
using TechNotes.Domain.Entities.Identity;
using Microsoft.IdentityModel.Tokens;
using TechNotes.Application.Interfaces.Services.Auth;
using Microsoft.Extensions.Options;



namespace TechNotes.Infrastructure.Services.Auth;
// Creates signed JWT access tokens. These tokens are short-lived and sent by the client in the Authorization header.
public sealed class JwtTokenService(IOptions<JwtOptions> options) : IJwtTokenService
{
    private readonly JwtOptions _options = options.Value;

    public AuthResult CreateAccessToken(AppUser user, IReadOnlyCollection<string> roles)
    {
        var expiresAtUtc = DateTime.UtcNow.AddMinutes(_options.AccessTokenMinutes);
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(JwtRegisteredClaimNames.Email, user.Email),
            new(ClaimTypes.Email, user.Email),
            new(ClaimTypes.Name, user.UserName)
        };

        claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

        var credentials = new SigningCredentials(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.Key)),
            SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _options.Issuer,
            audience: _options.Audience,
            claims: claims,
            expires: expiresAtUtc,
            signingCredentials: credentials);

        return new AuthResult(
            new JwtSecurityTokenHandler().WriteToken(token),
            expiresAtUtc,
            user.Id,
            user.UserName,
            user.Email,
            roles);
    }
}
