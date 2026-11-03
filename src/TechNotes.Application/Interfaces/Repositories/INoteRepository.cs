using TechNotes.Domain.Entities.Notes;

namespace TechNotes.Application.Interfaces.Repositories
{
    public interface INoteRepository : IGenericRepository<Note>
    {
        Task<Note?> GetWithContentAsync(Guid noteId, CancellationToken cancellationToken = default);

        Task<Note?> GetTrackedWithContentAsync(Guid noteId, CancellationToken cancellationToken = default);

        Task<IEnumerable<Note>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);

        Task<IEnumerable<Note>> GetAccessibleNotesAsync(Guid userId, CancellationToken cancellationToken = default);

        Task<IEnumerable<Note>> SearchAsync(Guid userId, string searchTerm, CancellationToken cancellationToken = default);

        Task<IEnumerable<Note>> GetPublicNotesAsync(CancellationToken cancellationToken = default);

        Task<IEnumerable<Note>> GetSharedWithMeAsync(Guid userId, CancellationToken cancellationToken = default);

        Task RemoveSectionsAsync(Note note, CancellationToken cancellationToken = default);

        Task RemoveTagsAsync(Note note, CancellationToken cancellationToken = default);

        Task AddTagsAsync(IEnumerable<NoteTag> noteTags, CancellationToken cancellationToken = default);
    }
}
