namespace TechNotes.Application.Common.Exceptions;

public sealed class ImageUploadFailedException(string message, Exception? innerException = null)
    : Exception(message, innerException);
