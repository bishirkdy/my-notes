using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using TechNotes.API.Authorization;
using TechNotes.Application.Settings.Auth;

namespace TechNotes.API.Extensions;

public static class JwtExtensions
{
    public static IServiceCollection AddJwtAuthentication(this IServiceCollection services,IConfiguration configuration)
    {
        var jwtOptions = configuration.GetSection(JwtOptions.SectionName).Get<JwtOptions>()
            ?? throw new InvalidOperationException("JWT settings are not configured.");

        if (string.IsNullOrWhiteSpace(jwtOptions.Key) ||
            jwtOptions.Key.Length < 32 ||
            jwtOptions.Key.StartsWith("CHANGE_ME", StringComparison.Ordinal))
        {
            throw new InvalidOperationException(
                "Configure a secure JWT key with at least 32 characters. Set JWT_KEY (or Jwt__Key) in the deployment environment.");
        }

        services.Configure<JwtOptions>(configuration.GetSection(JwtOptions.SectionName));

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.MapInboundClaims = true;
                options.TokenValidationParameters =
                    new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidIssuer = jwtOptions.Issuer,

                        ValidateAudience = true,
                        ValidAudience = jwtOptions.Audience,

                        ValidateLifetime = true,

                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey =
                            new SymmetricSecurityKey(
                                Encoding.UTF8.GetBytes(jwtOptions.Key)),

                        ClockSkew = TimeSpan.Zero
                    };

                options.Events = new JwtBearerEvents
                {
                    OnMessageReceived = context =>
                    {
                        if (string.IsNullOrWhiteSpace(context.Token) &&
                            context.Request.Cookies.TryGetValue(AuthCookieNames.AccessToken, out var accessToken))
                        {
                            context.Token = accessToken;
                        }

                        return Task.CompletedTask;
                    }
                };
            });

        return services;
    }
}