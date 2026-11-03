using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.Extensions.Options;
using TechNotes.Application.Common.Exceptions;
using TechNotes.Application.DTOs.Images;
using TechNotes.Application.Interfaces.Services.Images;
using TechNotes.Application.Settings.Images;

namespace TechNotes.Infrastructure.Services.Images;

public sealed class CloudinaryImageStorageService : IImageStorageService
{
    private readonly Cloudinary? _cloudinary;
    private readonly string _folder;

    public CloudinaryImageStorageService(IOptions<CloudinarySettings> options)
    {
        var settings = options.Value;
        _folder = string.IsNullOrWhiteSpace(settings.Folder) ? "technotes" : settings.Folder;

        if (string.IsNullOrWhiteSpace(settings.CloudName) ||
            string.IsNullOrWhiteSpace(settings.ApiKey) ||
            string.IsNullOrWhiteSpace(settings.ApiSecret))
        {
            return;
        }

        _cloudinary = new Cloudinary(new Account(
            settings.CloudName,
            settings.ApiKey,
            settings.ApiSecret));
    }

    public bool IsConfigured => _cloudinary is not null;

    public async Task<ImageUploadResponse> UploadAsync(
        Stream stream,
        string fileName,
        string contentType,
        CancellationToken cancellationToken = default)
    {
        if (_cloudinary is null)
        {
            throw new ImageStorageUnavailableException(
                "Image storage is not configured. Set Cloudinary credentials before uploading images.");
        }

        var parameters = new ImageUploadParams
        {
            File = new FileDescription(fileName, stream),
            Folder = _folder,
            UseFilename = true,
            UniqueFilename = true,
            Overwrite = false
        };

        try
        {
            var result = await _cloudinary.UploadAsync(parameters, cancellationToken);
            if (result.Error is not null)
            {
                throw new ImageUploadFailedException(
                    result.Error.Message ?? "Cloudinary rejected the image upload.");
            }

            return new ImageUploadResponse
            {
                Url = result.SecureUrl?.ToString() ?? string.Empty,
                PublicId = result.PublicId ?? string.Empty,
                Width = result.Width,
                Height = result.Height,
                Format = result.Format ?? contentType
            };
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (ImageUploadFailedException)
        {
            throw;
        }
        catch (Exception exception)
        {
            throw new ImageUploadFailedException(
                "The image could not be uploaded to Cloudinary.",
                exception);
        }
    }
}
