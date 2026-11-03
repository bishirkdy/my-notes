using MediatR;
using TechNotes.Application.DTOs.Notes;

namespace TechNotes.Application.Features.Notes.Queries.GetSharedNotes;

public sealed record GetSharedNotesQuery(Guid UserId)
    : IRequest<IReadOnlyList<NoteResponse>>;
