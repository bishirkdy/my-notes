using Microsoft.AspNetCore.Antiforgery;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using TechNotes.API.Authorization;
using TechNotes.API.Execution;
using TechNotes.API.Extensions;
using TechNotes.Application;
using TechNotes.Application.Interfaces.Services.Images;
using TechNotes.Application.Settings.Auth;
using TechNotes.Infrastructure;
using TechNotes.Persistence;
using TechNotes.Persistence.Data;

var builder = WebApplication.CreateBuilder(args);
var appliedAliases = builder.Configuration.AddFlatEnvironmentAliases();

builder.Services
    .AddApplication()
    .AddPersistence(builder.Configuration)
    .AddInfrastructure(builder.Configuration)
    .AddApiServices(builder.Configuration);

var (allowedOrigins, invalidOrigins) = ReadAllowedOrigins(builder.Configuration);
var crossSiteCookies = !builder.Environment.IsDevelopment();

builder.Services.AddAntiforgery(options =>
{
    options.HeaderName = "X-XSRF-TOKEN";
    options.Cookie.Name = AuthCookieNames.Csrf;
    options.Cookie.HttpOnly = true;
    options.Cookie.Path = AuthCookieNames.CsrfPath;
    options.Cookie.SecurePolicy = crossSiteCookies
        ? CookieSecurePolicy.Always
        : CookieSecurePolicy.SameAsRequest;
    options.Cookie.SameSite = crossSiteCookies
        ? SameSiteMode.None
        : SameSiteMode.Lax;
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("AngularClient", policy =>
    {
        policy
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();

        if (allowedOrigins.Length > 0)
        {
            policy.WithOrigins(allowedOrigins);
        }
    });
});

var app = builder.Build();

if (appliedAliases.Count > 0)
{
    app.Logger.LogInformation(
        "Applied flat environment aliases: {Aliases}.",
        string.Join(", ", appliedAliases));
}

if (invalidOrigins.Length > 0)
{
    app.Logger.LogWarning(
        "Ignored invalid CORS origins: {Origins}. Set FRONTEND_URL to a full http(s) origin.",
        string.Join(", ", invalidOrigins));
}

if (allowedOrigins.Length == 0)
{
    app.Logger.LogWarning(
        "No CORS origins are configured. Same-origin requests will still work; set FRONTEND_URL for cross-origin clients.");
}

LogOptionalServiceConfiguration(app);

app.UseApiPipeline();

app.MapGet("/health", () => Results.Ok(new { status = "ok" }));
app.MapGet("/health/ready", async (AppDbContext db, CancellationToken cancellationToken) =>
{
    try
    {
        if (await db.Database.CanConnectAsync(cancellationToken))
        {
            return Results.Ok(new { status = "ready" });
        }
    }
    catch (Exception exception)
    {
        app.Logger.LogWarning(exception, "Database readiness check failed.");
    }

    return Results.StatusCode(StatusCodes.Status503ServiceUnavailable);
});

await app.SeedDatabaseAsync();

app.Run();

static (string[] Allowed, string[] Invalid) ReadAllowedOrigins(IConfiguration configuration)
{
    var configured = configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];
    var allowed = new List<string>();
    var invalid = new List<string>();

    foreach (var raw in configured)
    {
        if (string.IsNullOrWhiteSpace(raw))
        {
            continue;
        }

        foreach (var part in raw.Split(
                     ',',
                     StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            var origin = part.Trim().TrimEnd('/');
            if (Uri.TryCreate(origin, UriKind.Absolute, out var uri) &&
                (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps))
            {
                if (!allowed.Contains(origin, StringComparer.OrdinalIgnoreCase))
                {
                    allowed.Add(origin);
                }
            }
            else
            {
                invalid.Add(part);
            }
        }
    }

    return (allowed.ToArray(), invalid.ToArray());
}

static void LogOptionalServiceConfiguration(WebApplication app)
{
    var imageStorage = app.Services.GetRequiredService<IImageStorageService>();
    if (!imageStorage.IsConfigured)
    {
        app.Logger.LogWarning(
            "Cloudinary is not configured. Image uploads will return 503. Set CLOUDINARY_CLOUD_NAME, CLOUDINARY_API_KEY, and CLOUDINARY_API_SECRET.");
    }

    var admin = app.Services.GetRequiredService<IOptions<AdminSeedOptions>>().Value;
    if (app.Configuration.GetValue<bool>("Database:SeedOnStartup") &&
        (string.IsNullOrWhiteSpace(admin.Password) ||
         admin.Password.StartsWith("CHANGE_ME", StringComparison.Ordinal)))
    {
        app.Logger.LogWarning(
            "Admin seeding is enabled but ADMIN_PASSWORD is not configured. No admin account will be created.");
    }

    var execution = app.Services.GetRequiredService<IOptions<ExecutionOptions>>().Value;
    app.Logger.LogInformation(
        "Code execution is {ExecutionState}.",
        execution.Enabled ? "enabled" : "disabled");
}
