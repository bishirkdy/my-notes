using MediatR;
using TechNotes.Application.DTOs.Notes;
using TechNotes.Application.Features.Notes.Common;
using TechNotes.Application.Interfaces.Repositories;
using TechNotes.Domain.Entities.Notes;
using TechNotes.Domain.Enums;

namespace TechNotes.Application.Features.Notes.Queries.GetNote
{
    public sealed class GetNoteQueryHandler(
        INoteRepository noteRepository,
        IReadingProgressRepository progressRepository)
        : IRequestHandler<GetNoteQuery, NoteResponse?>
    {
        public async Task<NoteResponse?> Handle(GetNoteQuery query, CancellationToken cancellationToken)
        {
            var note = await noteRepository.GetWithContentAsync(query.NoteId, cancellationToken);
            if (note is null || !CanRead(note, query.UserId))
            {
                return null;
            }

            var response = NoteResponseMapper.Map(note);
            if (query.UserId.HasValue)
            {
                var progress = await progressRepository.GetAsync(
                    query.UserId.Value,
                    note.Id,
                    cancellationToken);

                if (progress is not null)
                {
                    response.ReadingProgress = new ReadingProgressSummary
                    {
                        Percent = progress.Percent,
                        LastReadAtUtc = progress.LastReadAtUtc
                    };
                }
            }

            return response;
        }

        private static bool CanRead(Note note, Guid? userId)
        {
            if (userId.HasValue && note.UserId == userId.Value)
            {
                return true;
            }

            if (note.Visibility == NoteVisibility.Public && note.Status == NoteStatus.Published)
            {
                return true;
            }

            return userId.HasValue &&
                note.Visibility == NoteVisibility.Shared &&
                note.Shares.Any(share => share.UserId == userId.Value);
        }
    }
}
