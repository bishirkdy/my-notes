using MediatR;
using TechNotes.Application.DTOs.Notes;
using TechNotes.Application.Interfaces.Repositories;
using TechNotes.Domain.Entities.Notes;
using TechNotes.Domain.Enums;

namespace TechNotes.Application.Features.Notes.Commands.SaveReadingProgress;

public sealed class SaveReadingProgressCommandHandler(
    INoteRepository noteRepository,
    IReadingProgressRepository progressRepository)
    : IRequestHandler<SaveReadingProgressCommand, ReadingProgressResponse?>
{
    public async Task<ReadingProgressResponse?> Handle(
        SaveReadingProgressCommand command,
        CancellationToken cancellationToken)
    {
        var note = await noteRepository.GetByIdAsync(command.NoteId, cancellationToken);
        if (note is null || !CanRead(note, command.UserId))
        {
            return null;
        }

        var percent = Math.Clamp(command.Request.Percent, 0, 100);
        var progress = await progressRepository.GetAsync(
            command.UserId,
            command.NoteId,
            cancellationToken);

        if (progress is null)
        {
            progress = new ReadingProgress
            {
                UserId = command.UserId,
                NoteId = command.NoteId,
                Percent = percent,
                LastReadAtUtc = DateTime.UtcNow
            };
            await progressRepository.AddAsync(progress, cancellationToken);
        }
        else
        {
            progress.Percent = Math.Max(progress.Percent, percent);
            progress.LastReadAtUtc = DateTime.UtcNow;
            progressRepository.Update(progress);
        }

        await progressRepository.SaveChangesAsync(cancellationToken);

        return new ReadingProgressResponse
        {
            NoteId = progress.NoteId,
            Percent = progress.Percent,
            LastReadAtUtc = progress.LastReadAtUtc
        };
    }

    private static bool CanRead(Note note, Guid userId) =>
        note.UserId == userId ||
        (note.Visibility == NoteVisibility.Public && note.Status == NoteStatus.Published);
}
