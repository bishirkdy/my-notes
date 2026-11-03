namespace TechNotes.Application.Common.Exceptions;

public sealed class ImageStorageUnavailableException(string message)
    : Exception(message);
