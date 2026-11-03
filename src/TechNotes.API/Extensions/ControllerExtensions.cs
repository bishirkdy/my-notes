using Microsoft.AspNetCore.Http.Features;
using TechNotes.Application.Common.Images;

namespace TechNotes.API.Extensions;

public static class ControllerExtensions
{
    public static IServiceCollection AddApiControllers(this IServiceCollection services)
    {
        services.Configure<FormOptions>(options =>
        {
            options.MultipartBodyLengthLimit = ImageFileValidator.MaxFileSizeBytes + (512 * 1024);
        });
        services.AddControllers();
            //.ConfigureApiBehaviorOptions(options =>
            //{
            //    //InvalidModelStateResponseFactory lets you customize the response returned when model validation fails.
            //    options.InvalidModelStateResponseFactory = context =>
            //    {
            //        var problemDetails =
            //            new ValidationProblemDetails(context.ModelState)
            //            {
            //                Status = StatusCodes.Status400BadRequest,
            //                Title = "Validation failed.",
            //                Type = "https://httpstatuses.com/400",
            //                Instance = context.HttpContext.Request.Path
            //            };

            //        return new BadRequestObjectResult(problemDetails);
            //    };
            //});

        return services;
    }
}