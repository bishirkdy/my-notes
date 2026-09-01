using TechNotes.Domain.Entities.Notes;

namespace TechNotes.Application.Interfaces.Repositories
{
    // Defines database operations that are specific to the Note entity.
    public interface INoteRepository : IGenericRepository<Note>
    {
        // Retrieves a note with its complete section and content hierarchy.
        Task<Note?> GetWithContentAsync(Guid noteId, CancellationToken cancellationToken = default);

        // Retrieves notes owned by a specific user.
        Task<IEnumerable<Note>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);

        // Retrieves notes that are visible to a specific user.
        Task<IEnumerable<Note>> GetAccessibleNotesAsync(Guid userId, CancellationToken cancellationToken = default);

        // Searches notes belonging to a user by title or content.
        Task<IEnumerable<Note>> SearchAsync(Guid userId, string searchTerm, CancellationToken cancellationToken = default);

        // Retrieves publicly visible notes.
        Task<IEnumerable<Note>> GetPublicNotesAsync(CancellationToken cancellationToken = default);
    }
}