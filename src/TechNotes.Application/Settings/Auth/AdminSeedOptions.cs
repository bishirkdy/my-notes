namespace TechNotes.Application.Settings.Auth;

// Optional first-administrator settings. Keep the real password in user secrets, never source control.
public sealed class AdminSeedOptions
{
    public const string SectionName = "AdminSeed";

    public string Email { get; init; } = string.Empty;
    public string UserName { get; init; } = string.Empty;
    public string Password { get; init; } = string.Empty;
}
