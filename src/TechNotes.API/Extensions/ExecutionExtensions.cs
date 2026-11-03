using System.Threading.RateLimiting;
using Microsoft.AspNetCore.RateLimiting;
using TechNotes.API.Execution;

namespace TechNotes.API.Extensions;

public static class ExecutionExtensions
{
    public static IServiceCollection AddExecutionServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<ExecutionOptions>(configuration.GetSection(ExecutionOptions.SectionName));

        services.AddHttpClient<PistonClient>((serviceProvider, client) =>
        {
            var options = serviceProvider
                .GetRequiredService<Microsoft.Extensions.Options.IOptions<ExecutionOptions>>()
                .Value;

            client.BaseAddress = new Uri(options.PistonBaseUrl);
            client.Timeout = TimeSpan.FromSeconds(30);
        });

        services.AddSingleton<CSharpSnippetCompiler>();
        services.AddSingleton<SqlServerSandbox>();
        services.AddSingleton<PostgresSandbox>();
        services.AddHostedService<SqlSandboxInitializer>();
        services.AddSingleton<CodeExecutionService>();

        var executionOptions = configuration
            .GetSection(ExecutionOptions.SectionName)
            .Get<ExecutionOptions>() ?? new ExecutionOptions();

        services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
            options.OnRejected = (context, _) =>
            {
                context.HttpContext.Response.Headers.RetryAfter = "60";
                return ValueTask.CompletedTask;
            };
            options.AddPolicy("executions", context =>
            {
                var client = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";

                return RateLimitPartition.GetFixedWindowLimiter(
                    client,
                    _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = Math.Max(1, executionOptions.RateLimitPermitLimit),
                        Window = TimeSpan.FromMinutes(1),
                        QueueLimit = 0,
                        AutoReplenishment = true
                    });
            });
        });

        return services;
    }
}
