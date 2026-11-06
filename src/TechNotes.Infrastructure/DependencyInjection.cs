using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TechNotes.Application.Interfaces.Services.Auth;
using TechNotes.Application.Interfaces.Services.Images;
using TechNotes.Application.Settings.Images;
using TechNotes.Infrastructure.Services.Auth;
using TechNotes.Infrastructure.Services.Images;
using TechNotes.Infrastructure.Services.Security;

namespace TechNotes.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services , IConfiguration configuration)
        {
            //Auth
            services.AddScoped<IJwtTokenService, JwtTokenService>();
            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<IPasswordHasher, PasswordHasher>();

            //Images
            services.Configure<CloudinarySettings>(configuration.GetSection(CloudinarySettings.SectionName));
            services.AddSingleton<IImageStorageService, CloudinaryImageStorageService>();

            return services;
        }
    }
}
