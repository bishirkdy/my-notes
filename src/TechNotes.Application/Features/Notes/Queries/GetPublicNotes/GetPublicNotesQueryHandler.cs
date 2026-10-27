using MediatR;
using TechNotes.Application.DTOs.Notes;
using TechNotes.Application.Features.Notes.Common;
using TechNotes.Application.Interfaces.Repositories;

namespace TechNotes.Application.Features.Notes.Queries.GetPublicNotes
{
    public sealed class GetPublicNotesQueryHandler(INoteRepository noteRepository)
        : IRequestHandler<GetPublicNotesQuery, IReadOnlyList<NoteResponse>>
    {
        public async Task<IReadOnlyList<NoteResponse>> Handle(
            GetPublicNotesQuery query,
            CancellationToken cancellationToken)
        {
            var notes = await noteRepository.GetPublicNotesAsync(cancellationToken);
            return notes.Select(NoteResponseMapper.Map).ToList();
        }
    }
}
