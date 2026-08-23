using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using System.Text;
using TechNotes.API.Authorization;
using TechNotes.API.Common.Exceptions;
using TechNotes.Application.Interfaces.Services.Auth;
using TechNotes.Application.Settings.Auth;
using TechNotes.Domain.Constants;
using TechNotes.Domain.Entities.Identity;
using TechNotes.Infrastructure.Services.Auth;
using TechNotes.Infrastructure.Services.Security;
using TechNotes.Persistence.Data;
using TechNotes.Persistence.Seeder;


var builder = WebApplication.CreateBuilder(args);

// Reads the PostgreSQL connection settings from appsettings.json.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' is not configured.");

// Registers EF Core so services can use AppDbContext to access PostgreSQL.
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(connectionString));

// Binds token settings and stops the application if a real signing key was not supplied.
var jwtOptions = builder.Configuration.GetSection(JwtOptions.SectionName).Get<JwtOptions>()
    ?? throw new InvalidOperationException("JWT settings are not configured.");

if (jwtOptions.Key.Length < 32 || jwtOptions.Key.StartsWith("CHANGE_ME", StringComparison.Ordinal))
{
    throw new InvalidOperationException("Configure a secure JWT key with at least 32 characters in user secrets.");
}

builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection(JwtOptions.SectionName));
builder.Services.Configure<AdminSeedOptions>(builder.Configuration.GetSection(AdminSeedOptions.SectionName));

// PasswordHasher uses a modern, salted password hash. Passwords are never stored directly.
builder.Services.AddScoped<IPasswordHasher<AppUser>, PasswordHasher<AppUser>>();
builder.Services.AddScoped<IJwtTokenService, JwtTokenService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IPasswordHasher, PasswordHasher>();
builder.Services.AddScoped<DatabaseSeeder>();

// Validates JWT access tokens sent in the Authorization: Bearer header.
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwtOptions.Issuer,
            ValidateAudience = true,
            ValidAudience = jwtOptions.Audience,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.Key)),
            ClockSkew = TimeSpan.Zero
        };
    });

// Defines a reusable rule for endpoints that only administrators can access.
builder.Services.AddAuthorizationBuilder()
    .AddPolicy(AuthorizationPolicies.AdminOnly, policy => policy.RequireRole(RoleNames.Admin));

// Sends unhandled errors through one central handler and returns standard API error responses.
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

// Enables API controllers and creates a consistent response for invalid request models.
builder.Services.AddControllers()
    .ConfigureApiBehaviorOptions(options =>
    {
        options.InvalidModelStateResponseFactory = context =>
        {
            var problemDetails = new ValidationProblemDetails(context.ModelState)
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "Validation failed.",
                Type = "https://httpstatuses.com/400",
                Instance = context.HttpContext.Request.Path
            };

            return new BadRequestObjectResult(problemDetails);
        };
    });

// Generates the OpenAPI document used by Swagger UI to list and test API endpoints.
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    // Adds Swagger's Authorize button for pasting a JWT access token.
    options.AddSecurityDefinition("bearer", new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        Description = "Paste only the JWT access token. Swagger adds the Bearer prefix automatically."
    });
    options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
    {
        [new OpenApiSecuritySchemeReference("bearer", document)] = []
    });
});

var app = builder.Build();

// Swagger is available only in Development so API documentation is not exposed in production.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Handles exceptions before they reach controllers or expose internal implementation details.
app.UseExceptionHandler();

app.UseHttpsRedirection();

// Reads the JWT before authorization checks run.
app.UseAuthentication();
app.UseAuthorization();

// Activates endpoints defined in controllers.
app.MapControllers();

// Seeding is intentionally opt-in. Run migrations first, then set Database:SeedOnStartup to true once.
if (app.Configuration.GetValue<bool>("Database:SeedOnStartup"))
{
    using var scope = app.Services.CreateScope();
    await scope.ServiceProvider.GetRequiredService<DatabaseSeeder>().SeedAsync();
}

app.Run();
