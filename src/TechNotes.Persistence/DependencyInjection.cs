using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using TechNotes.Application.Interfaces.Repositories;
using TechNotes.Persistence.Data;
using TechNotes.Persistence.Repositories;
using TechNotes.Persistence.Repositories.Auth;
using TechNotes.Persistence.Seeder;

namespace TechNotes.Persistence
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddPersistence(this IServiceCollection services , IConfiguration configuration)
        {
            // Reads the PostgreSQL connection settings from the deployment environment.
            var connectionString = configuration.GetConnectionString("DefaultConnection")?.Trim();
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new InvalidOperationException(
                    "Database connection is not configured. Set NEON_CONNECTION_STRING (or ConnectionStrings__DefaultConnection) in the deployment environment.");
            }

            try
            {
                _ = new NpgsqlConnectionStringBuilder(connectionString);
            }
            catch (ArgumentException exception)
            {
                throw new InvalidOperationException(
                    "Database connection string is invalid. Check NEON_CONNECTION_STRING (or ConnectionStrings__DefaultConnection).",
                    exception);
            }

            // Registers EF Core so services can use AppDbContext to access PostgreSQL.
            services.AddDbContext<AppDbContext>(options =>
                options.UseNpgsql(
                    connectionString,
                    npgsql => npgsql.EnableRetryOnFailure(
                        5,
                        TimeSpan.FromSeconds(5),
                        null)));
            
            //DB Seed
            services.AddScoped<DatabaseSeeder>();

            //generic
            services.AddScoped(typeof(IGenericRepository<>),typeof(GenericRepository<>));
            
            //Note
            services.AddScoped<INoteRepository, NoteRepository>();
            services.AddScoped<IReadingProgressRepository, ReadingProgressRepository>();
            services.AddScoped<ICategoryRepository, CategoryRepository>();
            services.AddScoped<ITagRepository, TagRepository>();

            //Auth
            services.AddScoped<IAuthRepository, AuthRepository>();
            return services;
        }
    }
}
