using MediatR;
using TechNotes.Application.DTOs.Notes;
using TechNotes.Application.Features.Notes.Common;
using TechNotes.Application.Interfaces.Repositories;
using TechNotes.Domain.Enums;

namespace TechNotes.Application.Features.Notes.Queries.GetNote
{
    public sealed class GetNoteQueryHandler(INoteRepository noteRepository)
        : IRequestHandler<GetNoteQuery, NoteResponse?>
    {
        public async Task<NoteResponse?> Handle(GetNoteQuery query, CancellationToken cancellationToken)
        {
            var note = await noteRepository.GetWithContentAsync(query.NoteId, cancellationToken);
            if (note is null)
            {
                return null;
            }

            if (query.UserId.HasValue && note.UserId == query.UserId.Value)
            {
                return NoteResponseMapper.Map(note);
            }

            if (note.Visibility == NoteVisibility.Public && note.Status == NoteStatus.Published)
            {
                return NoteResponseMapper.Map(note);
            }

            if (query.UserId.HasValue &&
                note.Visibility == NoteVisibility.Shared &&
                note.Shares.Any(share => share.UserId == query.UserId.Value))
            {
                return NoteResponseMapper.Map(note);
            }

            return null;
        }
    }
}
