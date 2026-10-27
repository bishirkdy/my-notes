using TechNotes.API.Extensions;

public static class ApplicationExtensions
{
    public static WebApplication UseApiPipeline(this WebApplication app)
    {
        app.UseSwaggerDocumentation();
        app.UseExceptionHandling();
        app.UseHttpsRedirection();

        // CORS must run before authentication so preflight requests are handled.
        app.UseCors("AngularClient");
        app.UseAuthentication();
        app.UseAuthorization();
        app.MapControllers();

        return app;
    }
}
