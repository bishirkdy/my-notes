using MediatR;
using TechNotes.Application.Interfaces.Repositories;

namespace TechNotes.Application.Features.Notes.Commands.DeleteNote
{
    // Handles deleting a note owned by the authenticated user.
    public sealed class DeleteNoteCommandHandler(
        INoteRepository noteRepository) : IRequestHandler<DeleteNoteCommand, bool>
    {
        // Deletes the note after verifying ownership.
        public async Task<bool> Handle(DeleteNoteCommand command, CancellationToken cancellationToken)
        {
            var note = await noteRepository.GetByIdAsync(command.NoteId,cancellationToken);
            if (note is null || note.UserId != command.UserId)
            {
                return false;
            }

            noteRepository.Delete(note);
            await noteRepository.SaveChangesAsync(cancellationToken);
            return true;
        }
    }
}
