using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace TechNotes.Persistence.Data;

public sealed class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    private const string UserSecretsId = "technotes-api-9dba6e6c-03b6-48ab-a5f7-3de75d8273e7";

    public AppDbContext CreateDbContext(string[] args)
    {
        var apiPath = ResolveApiProjectPath();
        var configuration = new ConfigurationBuilder()
            .SetBasePath(apiPath)
            .AddJsonFile("appsettings.json", optional: false)
            .AddJsonFile("appsettings.Development.json", optional: true)
            .AddUserSecrets(UserSecretsId)
            .AddEnvironmentVariables()
            .Build();

        var connectionString = configuration.GetConnectionString("DefaultConnection");
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException(
                "Connection string 'DefaultConnection' is not configured. Set it with dotnet user-secrets.");
        }

        var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
        optionsBuilder.UseNpgsql(connectionString);
        return new AppDbContext(optionsBuilder.Options);
    }

    private static string ResolveApiProjectPath()
    {
        var directory = new DirectoryInfo(Directory.GetCurrentDirectory());

        while (directory is not null)
        {
            var nested = Path.Combine(directory.FullName, "src", "TechNotes.API");
            if (Directory.Exists(nested))
            {
                return nested;
            }

            var sibling = Path.Combine(directory.FullName, "TechNotes.API");
            if (Directory.Exists(sibling))
            {
                return sibling;
            }

            directory = directory.Parent;
        }

        throw new DirectoryNotFoundException("Could not locate the TechNotes.API project directory.");
    }
}
