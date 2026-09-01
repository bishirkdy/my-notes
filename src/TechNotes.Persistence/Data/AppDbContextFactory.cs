using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace TechNotes.Persistence.Data;

// Creates AppDbContext when EF Core commands run at design time.
public sealed class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        // Find the solution's src directory from the current EF working directory.
        var srcPath = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), ".."));

        // Load configuration from the API project's appsettings.json.
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Path.Combine(srcPath, "TechNotes.API"))
            .AddJsonFile("appsettings.json", optional: false)
            .AddJsonFile("appsettings.Development.json", optional: true).AddUserSecrets("TechNotes.API")
            .Build();

        // Read the PostgreSQL connection string.
        var connectionString = configuration.GetConnectionString("DefaultConnection");

        // Stop with a clear error if the connection string is missing.
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException(
                "Connection string 'DefaultConnection' is not configured.");
        }

        // Configure EF Core to use PostgreSQL.
        var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
        optionsBuilder.UseNpgsql(connectionString);

        return new AppDbContext(optionsBuilder.Options);
    }
}