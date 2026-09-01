using TechNotes.Application.DTOs.Notes;

namespace TechNotes.Application.Interfaces.Services.Business.Notes
{
    // Defines business operations for creating, reading, updating, and deleting notes.
    public interface INoteService
    {
        // Creates a new note for the authenticated user.
        Task<NoteResponse> CreateAsync(Guid userId,CreateNoteRequest request,CancellationToken cancellationToken = default);

        // Retrieves a note when the authenticated user has permission to view it.
        Task<NoteResponse?> GetByIdAsync(Guid noteId,Guid userId,CancellationToken cancellationToken = default);

        // Retrieves notes owned by the authenticated user.
        Task<IReadOnlyList<NoteResponse>> GetMyNotesAsync(Guid userId,CancellationToken cancellationToken = default);

        // Updates a note when the authenticated user has edit permission.
        Task<NoteResponse?> UpdateAsync(Guid noteId,Guid userId,UpdateNoteRequest request,CancellationToken cancellationToken = default);

        // Deletes a note when the authenticated user is the owner.
        Task<bool> DeleteAsync(Guid noteId,Guid userId,CancellationToken cancellationToken = default);
    }
}
