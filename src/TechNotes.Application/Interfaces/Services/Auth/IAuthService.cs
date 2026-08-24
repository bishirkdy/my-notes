using TechNotes.Application.DTOs.Auth;
using TechNotes.Application.Results.Auth;

namespace TechNotes.Application.Interfaces.Services.Auth;
// Defines the account operations used by the authentication controller.
public interface IAuthService
{
    Task<RefreshResult> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken);
    Task<RefreshResult?> LoginAsync(LoginRequest request, CancellationToken cancellationToken);
    Task<RefreshResult?> RefreshAsync(string refreshToken, CancellationToken cancellationToken);
    Task LogoutAsync(string refreshToken, CancellationToken cancellationToken);
}
