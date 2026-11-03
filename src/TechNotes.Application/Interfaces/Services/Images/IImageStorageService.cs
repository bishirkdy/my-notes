using TechNotes.Application.DTOs.Images;

namespace TechNotes.Application.Interfaces.Services.Images;

public interface IImageStorageService
{
    bool IsConfigured { get; }

    Task<ImageUploadResponse> UploadAsync(
        Stream stream,
        string fileName,
        string contentType,
        CancellationToken cancellationToken = default);
}
