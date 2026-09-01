namespace TechNotes.API.Extensions
{
    public static class MediatRExterntions
    {
        public static IServiceCollection AddMediatR(this IServiceCollection services)
        {
            // Registers all MediatR handlers from the Application assembly.
            services.AddMediatR(opt =>
            {
                opt.RegisterServicesFromAssemblies(typeof(TechNotes.Application.AssemblyReference).Assembly);
            });
            return services;
        }
    }
}
