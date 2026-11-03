using MediatR;
using TechNotes.Application.DTOs.Notes;

namespace TechNotes.Application.Features.Notes.Queries.GetPublicNotes
{
    // Represents a request to retrieve publicly visible notes.
    public sealed record GetPublicNotesQuery(Guid? UserId = null) : IRequest<IReadOnlyList<NoteResponse>>;
}
