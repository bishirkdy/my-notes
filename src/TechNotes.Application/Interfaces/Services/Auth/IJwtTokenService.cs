using TechNotes.Application.Results.Auth;
using TechNotes.Domain.Entities.Identity;

namespace TechNotes.Application.Interfaces.Services.Auth;

// Creates short-lived access tokens containing the user's ID and roles.
public interface IJwtTokenService
{
    AuthResult CreateAccessToken(AppUser user, IReadOnlyCollection<string> roles);
}
