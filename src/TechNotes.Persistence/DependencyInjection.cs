using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
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
            // Reads the PostgreSQL connection settings from appsettings.json.
            var connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("Connection string 'DefaultConnection' is not configured.");
            // Registers EF Core so services can use AppDbContext to access PostgreSQL.
            services.AddDbContext<AppDbContext>(options => options.UseNpgsql(connectionString));
            
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
