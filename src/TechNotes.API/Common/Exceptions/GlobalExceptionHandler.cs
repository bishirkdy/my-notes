using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;


namespace TechNotes.API.Common.Exceptions;

// Converts unhandled exceptions into consistent, safe API error responses.
public sealed class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    //This is the method ASP.NET Core calls when an unhandled exception occurs. ValueTask<bool> - The method eventually returns a boolean.
    //true  → Exception was handled and false → Exception was not handled
    //.NET Core internally calls TryHandleAsync() when an unhandled exception occurs.
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext,Exception exception,CancellationToken cancellationToken)
    {
        // Keeps full technical details in the server log, not in the response sent to users.
        logger.LogError(exception, "An unhandled exception occurred while processing {Path}.", httpContext.Request.Path);

        // Maps expected application errors to useful HTTP status codes.
        var (status, title, detail) = exception switch
        {
            ArgumentException => (StatusCodes.Status400BadRequest, "Invalid request.", exception.Message),
            KeyNotFoundException => (StatusCodes.Status404NotFound, "Resource not found.", exception.Message),
            _ => (StatusCodes.Status500InternalServerError, "An unexpected error occurred.", "Please try again later.")
        };

        // ProblemDetails is the standard JSON format used for API errors.
        var problemDetails = new ProblemDetails
        {
            Status = status,
            Title = title,
            Detail = detail,
            Instance = httpContext.Request.Path
        };

        httpContext.Response.StatusCode = status;
        httpContext.Response.ContentType = "application/problem+json";
        await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);

        return true;
    }
}
