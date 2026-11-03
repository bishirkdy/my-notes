using Microsoft.AspNetCore.HttpOverrides;
using TechNotes.API.Extensions;

public static class ApplicationExtensions
{
    public static WebApplication UseApiPipeline(this WebApplication app)
    {
        var forwardedHeaders = new ForwardedHeadersOptions
        {
            ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto,
            ForwardLimit = 1
        };
        forwardedHeaders.KnownIPNetworks.Clear();
        forwardedHeaders.KnownProxies.Clear();

        app.UseForwardedHeaders(forwardedHeaders);
        app.UseSwaggerDocumentation();
        app.UseExceptionHandling();

        if (app.Configuration.GetValue("HttpsRedirection:Enabled", true))
        {
            app.UseHttpsRedirection();
        }

        // CORS must run before authentication so preflight requests are handled.
        app.UseCors("AngularClient");
        app.UseRateLimiter();
        app.UseAuthentication();
        app.UseAuthorization();
        app.MapControllers();

        return app;
    }
}
