using MediatR;
using TechNotes.Application.DTOs.Notes;
using TechNotes.Application.Features.Notes.Common;
using TechNotes.Application.Interfaces.Repositories;

namespace TechNotes.Application.Features.Notes.Queries.GetPublicNotes
{
    public sealed class GetPublicNotesQueryHandler(
        INoteRepository noteRepository,
        IReadingProgressRepository progressRepository)
        : IRequestHandler<GetPublicNotesQuery, IReadOnlyList<NoteResponse>>
    {
        public async Task<IReadOnlyList<NoteResponse>> Handle(
            GetPublicNotesQuery query,
            CancellationToken cancellationToken)
        {
            var notes = (await noteRepository.GetPublicNotesAsync(cancellationToken)).ToList();
            var responses = notes.Select(NoteResponseMapper.Map).ToList();

            if (!query.UserId.HasValue)
            {
                return responses;
            }

            var progress = await progressRepository.GetForNotesAsync(
                query.UserId.Value,
                notes.Select(note => note.Id).ToList(),
                cancellationToken);

            for (var index = 0; index < notes.Count; index++)
            {
                if (progress.TryGetValue(notes[index].Id, out var item))
                {
                    responses[index].ReadingProgress = new ReadingProgressSummary
                    {
                        Percent = item.Percent,
                        LastReadAtUtc = item.LastReadAtUtc
                    };
                }
            }

            return responses;
        }
    }
}
