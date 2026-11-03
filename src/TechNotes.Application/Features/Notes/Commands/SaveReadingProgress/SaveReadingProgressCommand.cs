using MediatR;
using TechNotes.Application.DTOs.Notes;

namespace TechNotes.Application.Features.Notes.Commands.SaveReadingProgress;

public sealed record SaveReadingProgressCommand(
    Guid NoteId,
    Guid UserId,
    SaveReadingProgressRequest Request) : IRequest<ReadingProgressResponse?>;
