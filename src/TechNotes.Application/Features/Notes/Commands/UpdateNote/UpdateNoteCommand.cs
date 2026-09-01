

using MediatR;
using TechNotes.Application.DTOs.Notes;

namespace TechNotes.Application.Features.Notes.Commands.UpdateNote
{
    // Represents a request to update an existing note.
    public sealed record UpdateNoteCommand(Guid NoteId, Guid UserId, UpdateNoteRequest Request) : IRequest<NoteResponse?>;
}
