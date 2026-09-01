using MediatR;
using TechNotes.Application.DTOs.Notes;

namespace TechNotes.Application.Features.Notes.Commands.CreateNote
{
    // Represents the request to create a new note.
    public sealed record CreateNoteCommand(Guid UserId, CreateNoteRequest Request) : IRequest<NoteResponse>;
    
}
