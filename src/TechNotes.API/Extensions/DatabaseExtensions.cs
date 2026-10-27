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

        if (app.Configuration.GetValue<bool>("Database:ApplyMigrationsOnStartup"))
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            await dbContext.Database.MigrateAsync();
        }

        if (!app.Configuration.GetValue<bool>("Database:SeedOnStartup"))
        {
            return;
        }

        await scope.ServiceProvider.GetRequiredService<DatabaseSeeder>().SeedAsync();
    }
}
