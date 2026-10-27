using MediatR;
using TechNotes.Application.DTOs.Notes;
using TechNotes.Application.Features.Notes.Common;
using TechNotes.Application.Interfaces.Repositories;

namespace TechNotes.Application.Features.Notes.Queries.SearchNotes
{
    public sealed class SearchNotesQueryHandler(INoteRepository noteRepository)
        : IRequestHandler<SearchNotesQuery, IReadOnlyList<NoteResponse>>
    {
        public async Task<IReadOnlyList<NoteResponse>> Handle(
            SearchNotesQuery query,
            CancellationToken cancellationToken)
        {
            var notes = await noteRepository.SearchAsync(query.UserId, query.SearchTerm, cancellationToken);
            return notes.Select(NoteResponseMapper.Map).ToList();
        }
    }
}
