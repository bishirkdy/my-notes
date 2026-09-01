using MediatR;
using TechNotes.Application.DTOs.Notes;

namespace TechNotes.Application.Features.Notes.Queries.GetNote
{
    // Represents a request to retrieve a single note.
    public sealed record GetNoteQuery(Guid NoteId, Guid? UserId) : IRequest<NoteResponse?>;
}
