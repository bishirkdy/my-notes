using MediatR;

namespace TechNotes.Application.Features.Notes.Commands.DeleteNote
{
    // Represents a request to delete a note.
    public sealed record DeleteNoteCommand(Guid NoteId, Guid UserId, bool IsAdmin) : IRequest<bool>;
}
