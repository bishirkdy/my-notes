using Microsoft.EntityFrameworkCore;
using TechNotes.Application.Settings.Auth;
using TechNotes.Persistence.Data;
using TechNotes.Persistence.Seeder;

namespace TechNotes.API.Extensions;

public static class DatabaseExtensions
{
    public static IServiceCollection AddDatabaseSeeding(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<AdminSeedOptions>(configuration.GetSection(AdminSeedOptions.SectionName));
        return services;
    }

    public static async Task SeedDatabaseAsync(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();

        var applyMigrations = app.Configuration.GetValue<bool>("Database:ApplyMigrationsOnStartup");
        var seedDatabase = app.Configuration.GetValue<bool>("Database:SeedOnStartup");

        if (!applyMigrations && seedDatabase)
        {
            app.Logger.LogWarning(
                "Database seeding is enabled while migrations are disabled. Ensure the schema already exists.");
        }

        try
        {
            if (applyMigrations)
            {
                app.Logger.LogInformation("Applying database migrations.");
                var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                await dbContext.Database.MigrateAsync();
            }

            if (!seedDatabase)
            {
                app.Logger.LogInformation("Database seeding is disabled.");
                return;
            }

            app.Logger.LogInformation("Seeding database.");
            await scope.ServiceProvider.GetRequiredService<DatabaseSeeder>().SeedAsync();
        }
        catch (Exception exception)
        {
            app.Logger.LogCritical(
                exception,
                "Database initialization failed. Check NEON_CONNECTION_STRING (or ConnectionStrings__DefaultConnection) and the Neon database status.");
            throw;
        }
    }
}
