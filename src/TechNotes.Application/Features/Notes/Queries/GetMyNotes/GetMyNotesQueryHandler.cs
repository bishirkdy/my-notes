using MediatR;
using TechNotes.Application.DTOs.Notes;
using TechNotes.Application.Features.Notes.Common;
using TechNotes.Application.Interfaces.Repositories;

namespace TechNotes.Application.Features.Notes.Queries.GetMyNotes
{
    public sealed class GetMyNotesQueryHandler(INoteRepository noteRepository)
        : IRequestHandler<GetMyNotesQuery, IReadOnlyList<NoteResponse>>
    {
        public async Task<IReadOnlyList<NoteResponse>> Handle(
            GetMyNotesQuery query,
            CancellationToken cancellationToken)
        {
            var notes = await noteRepository.GetByUserIdAsync(query.UserId, cancellationToken);
            return notes.Select(NoteResponseMapper.Map).ToList();
        }
    }
}
