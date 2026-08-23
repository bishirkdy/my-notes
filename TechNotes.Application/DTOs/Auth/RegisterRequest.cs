using System.ComponentModel.DataAnnotations;

namespace TechNotes.Application.DTOs.Auth;
// Data accepted when a normal user creates an account.
public sealed class RegisterRequest
{
    [Required, StringLength(100, MinimumLength = 3)]
    public string UserName { get; init; } = string.Empty;

    [Required, EmailAddress, StringLength(256)]
    public string Email { get; init; } = string.Empty;

    [Required, StringLength(100, MinimumLength = 8)]
    public string Password { get; init; } = string.Empty;
}
