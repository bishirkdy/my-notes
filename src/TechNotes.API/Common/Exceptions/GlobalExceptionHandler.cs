using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TechNotes.Application.Common.Exceptions;

namespace TechNotes.API.Common.Exceptions;

public sealed class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        logger.LogError(exception, "Unhandled exception for {Path}.", httpContext.Request.Path);

        var (status, title, detail) = exception switch
        {
            ArgumentException or ValidationException =>
                (StatusCodes.Status400BadRequest, "Invalid request.", exception.Message),
            UnauthorizedAccessException =>
                (StatusCodes.Status401Unauthorized, "Unauthorized.", "You are not allowed to perform this action."),
            ConflictException =>
                (StatusCodes.Status409Conflict, "Conflict.", exception.Message),
            KeyNotFoundException =>
                (StatusCodes.Status404NotFound, "Resource not found.", exception.Message),
            ImageStorageUnavailableException =>
                (StatusCodes.Status503ServiceUnavailable, "Image storage unavailable.", exception.Message),
            ImageUploadFailedException =>
                (StatusCodes.Status502BadGateway, "Image upload failed.", exception.Message),
            DbUpdateException =>
                (StatusCodes.Status409Conflict, "Database conflict.", "The operation could not be completed."),
            OperationCanceledException =>
                (StatusCodes.Status499ClientClosedRequest, "Request cancelled.", "The request was cancelled."),
            _ => (StatusCodes.Status500InternalServerError, "Unexpected error.", "Please try again later.")
        };

        if (httpContext.Response.HasStarted)
        {
            return false;
        }

        httpContext.Response.StatusCode = status;
        httpContext.Response.ContentType = "application/problem+json";

        var problemDetails = new ProblemDetails
        {
            Status = status,
            Title = title,
            Detail = detail,
            Instance = httpContext.Request.Path
        };

        await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);
        return true;
    }
}
