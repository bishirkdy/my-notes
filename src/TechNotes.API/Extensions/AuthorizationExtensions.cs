using TechNotes.API.Authorization;
using TechNotes.Domain.Constants;

namespace TechNotes.API.Extensions;
public static class AuthorizationExtensions
{
    public static IServiceCollection AddAuthorizationPolicies(this IServiceCollection services)
    {
        services.AddAuthorizationBuilder().AddPolicy(AuthorizationPolicies.AdminOnly,policy => policy.RequireRole(RoleNames.Admin));
        return services;
    }
}