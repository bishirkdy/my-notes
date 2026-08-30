using TechNotes.Application.Settings.Auth;
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
        if (!app.Configuration.GetValue<bool>("Database:SeedOnStartup"))
            return;

        using var scope = app.Services.CreateScope();
        await scope.ServiceProvider.GetRequiredService<DatabaseSeeder>().SeedAsync();
    }
}