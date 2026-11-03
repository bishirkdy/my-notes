using MediatR;
using TechNotes.Application.DTOs.Notes;
using TechNotes.Application.Features.Notes.Common;
using TechNotes.Application.Interfaces.Repositories;

namespace TechNotes.Application.Features.Notes.Queries.GetSharedNotes;

public sealed class GetSharedNotesQueryHandler(INoteRepository noteRepository)
    : IRequestHandler<GetSharedNotesQuery, IReadOnlyList<NoteResponse>>
{
    public async Task<IReadOnlyList<NoteResponse>> Handle(
        GetSharedNotesQuery query,
        CancellationToken cancellationToken)
    {
        var notes = await noteRepository.GetSharedWithMeAsync(query.UserId, cancellationToken);
        return notes.Select(NoteResponseMapper.Map).ToList();
    }
}
