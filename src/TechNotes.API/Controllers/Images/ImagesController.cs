using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TechNotes.API.Authorization;
using TechNotes.Application.Common.Images;
using TechNotes.Application.DTOs.Images;
using TechNotes.Application.Interfaces.Services.Images;

namespace TechNotes.API.Controllers.Images;

[ApiController]
[Route("api/v1/images")]
public sealed class ImagesController(IImageStorageService imageStorage) : ControllerBase
{
    private const int MaxMultipartBytes = (int)ImageFileValidator.MaxFileSizeBytes + (512 * 1024);

    [Authorize(Policy = AuthorizationPolicies.AdminOnly)]
    [HttpPost]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(MaxMultipartBytes)]
    [ProducesResponseType<ImageUploadResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
    public async Task<ActionResult<ImageUploadResponse>> Upload(
        [FromForm] IFormFile? file,
        CancellationToken cancellationToken)
    {
        if (file is null)
        {
            return BadRequest(new ProblemDetails
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "Image file is required.",
                Detail = "Select a JPEG, PNG, WebP, or GIF image to upload."
            });
        }

        await using var uploadStream = new MemoryStream();
        await file.CopyToAsync(uploadStream, cancellationToken);

        var headerLength = (int)Math.Min(12, uploadStream.Length);
        var header = uploadStream.GetBuffer().AsSpan(0, headerLength);
        ImageFileValidator.Validate(file.FileName, file.ContentType, uploadStream.Length, header);

        uploadStream.Position = 0;
        var result = await imageStorage.UploadAsync(
            uploadStream,
            Path.GetFileName(file.FileName),
            file.ContentType,
            cancellationToken);

        return Ok(result);
    }
}
