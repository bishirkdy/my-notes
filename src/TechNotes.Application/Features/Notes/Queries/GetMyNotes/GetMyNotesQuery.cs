using MediatR;
using TechNotes.Application.DTOs.Notes;

namespace TechNotes.Application.Features.Notes.Queries.GetMyNotes
{
    // Represents a request to retrieve notes owned by the authenticated user.
    public sealed record GetMyNotesQuery(Guid UserId) : IRequest<IReadOnlyList<NoteResponse>>;
}
