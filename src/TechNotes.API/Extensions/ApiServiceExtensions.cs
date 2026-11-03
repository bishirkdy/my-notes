namespace TechNotes.API.Extensions;

public static class ApiServiceExtensions
{
    public static IServiceCollection AddApiServices(this IServiceCollection services, IConfiguration configuration)
    {
        services
            .AddJwtAuthentication(configuration)
            .AddAuthorizationPolicies()
            .AddExceptionHandling()
            .AddApiControllers()
            .AddDatabaseSeeding(configuration)
            .AddSwaggerDocumentation()
            .AddMediatR()
            .AddExecutionServices(configuration);

        return services;
    }
}