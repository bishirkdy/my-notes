using Microsoft.Extensions.Configuration;

namespace TechNotes.API.Extensions;

public static class ConfigurationAliasExtensions
{
    private static readonly (string TargetKey, string AspNetVariable, string FlatVariable)[] Aliases =
    [
        ("Jwt:Key", "Jwt__Key", "JWT_KEY"),
        ("Jwt:Issuer", "Jwt__Issuer", "JWT_ISSUER"),
        ("Jwt:Audience", "Jwt__Audience", "JWT_AUDIENCE"),
        ("ConnectionStrings:DefaultConnection", "ConnectionStrings__DefaultConnection", "NEON_CONNECTION_STRING"),
        ("Cors:AllowedOrigins:0", "Cors__AllowedOrigins__0", "FRONTEND_URL"),
        ("AdminSeed:Email", "AdminSeed__Email", "ADMIN_EMAIL"),
        ("AdminSeed:UserName", "AdminSeed__UserName", "ADMIN_USERNAME"),
        ("AdminSeed:Password", "AdminSeed__Password", "ADMIN_PASSWORD"),
        ("Cloudinary:CloudName", "Cloudinary__CloudName", "CLOUDINARY_CLOUD_NAME"),
        ("Cloudinary:ApiKey", "Cloudinary__ApiKey", "CLOUDINARY_API_KEY"),
        ("Cloudinary:ApiSecret", "Cloudinary__ApiSecret", "CLOUDINARY_API_SECRET"),
        ("Cloudinary:Folder", "Cloudinary__Folder", "CLOUDINARY_FOLDER"),
        ("Database:ApplyMigrationsOnStartup", "Database__ApplyMigrationsOnStartup", "APPLY_MIGRATIONS_ON_STARTUP"),
        ("Database:SeedOnStartup", "Database__SeedOnStartup", "SEED_ON_STARTUP"),
        ("Execution:RateLimitPermitLimit", "Execution__RateLimitPermitLimit", "EXECUTION_RATE_LIMIT_PERMIT_LIMIT"),
        ("Execution:Enabled", "Execution__Enabled", "EXECUTION_ENABLED"),
        ("Execution:PistonBaseUrl", "Execution__PistonBaseUrl", "PISTON_BASE_URL"),
        ("HttpsRedirection:Enabled", "HttpsRedirection__Enabled", "HTTPS_REDIRECTION_ENABLED")
    ];

    public static IReadOnlyList<string> AddFlatEnvironmentAliases(this IConfigurationBuilder configuration)
    {
        var values = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);
        var applied = new List<string>();

        foreach (var alias in Aliases)
        {
            if (!string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable(alias.AspNetVariable)))
            {
                continue;
            }

            var value = Normalize(Environment.GetEnvironmentVariable(alias.FlatVariable));
            if (string.IsNullOrWhiteSpace(value))
            {
                continue;
            }

            values[alias.TargetKey] = value;
            applied.Add(alias.FlatVariable);
        }

        if (values.Count > 0)
        {
            configuration.AddInMemoryCollection(values);
        }

        return applied;
    }

    private static string? Normalize(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        var trimmed = value.Trim();
        if (trimmed.Length >= 2 &&
            ((trimmed[0] == '"' && trimmed[^1] == '"') ||
             (trimmed[0] == '\'' && trimmed[^1] == '\'')))
        {
            trimmed = trimmed[1..^1].Trim();
        }

        return trimmed;
    }
}
