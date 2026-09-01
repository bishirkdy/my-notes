using Microsoft.Extensions.DependencyInjection;
using TechNotes.Application.Interfaces.Services.Business.Notes;
using TechNotes.Application.Service;

namespace TechNotes.Application
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplication(this IServiceCollection services)
        {
            //Di for note service
            services.AddScoped<INoteService, NoteService>();
            
            return services;
        }
    }
}
