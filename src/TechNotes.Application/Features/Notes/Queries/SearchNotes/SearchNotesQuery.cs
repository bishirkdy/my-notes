using MediatR;
using TechNotes.Application.DTOs.Notes;

namespace TechNotes.Application.Features.Notes.Queries.SearchNotes
{
    // Represents a request to search notes owned by the authenticated user.
    public sealed record SearchNotesQuery(Guid UserId, string SearchTerm) : IRequest<IReadOnlyList<NoteResponse>>;
}
