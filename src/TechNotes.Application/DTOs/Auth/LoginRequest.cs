using System.ComponentModel.DataAnnotations;

namespace TechNotes.Application.DTOs.Auth;
// Data accepted when a user signs in with email and password.
public sealed class LoginRequest
{
    [Required, EmailAddress]
    public string Email { get; init; } = string.Empty;

    [Required]
    public string Password { get; init; } = string.Empty;
}
