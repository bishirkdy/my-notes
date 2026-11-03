using TechNotes.Domain.Entities.Notes;

namespace TechNotes.Application.Interfaces.Repositories;

public interface IReadingProgressRepository : IGenericRepository<ReadingProgress>
{
    Task<ReadingProgress?> GetAsync(
        Guid userId,
        Guid noteId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyDictionary<Guid, ReadingProgress>> GetForNotesAsync(
        Guid userId,
        IReadOnlyCollection<Guid> noteIds,
        CancellationToken cancellationToken = default);
}
