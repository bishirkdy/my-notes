using Microsoft.EntityFrameworkCore;
using TechNotes.Application.Interfaces.Repositories;
using TechNotes.Domain.Entities.Notes;
using TechNotes.Persistence.Data;

namespace TechNotes.Persistence.Repositories;

public sealed class ReadingProgressRepository(AppDbContext context)
    : GenericRepository<ReadingProgress>(context), IReadingProgressRepository
{
    public Task<ReadingProgress?> GetAsync(
        Guid userId,
        Guid noteId,
        CancellationToken cancellationToken = default) =>
        DbSet.FirstOrDefaultAsync(
            progress => progress.UserId == userId && progress.NoteId == noteId,
            cancellationToken);

    public async Task<IReadOnlyDictionary<Guid, ReadingProgress>> GetForNotesAsync(
        Guid userId,
        IReadOnlyCollection<Guid> noteIds,
        CancellationToken cancellationToken = default)
    {
        if (noteIds.Count == 0)
        {
            return new Dictionary<Guid, ReadingProgress>();
        }

        var progress = await DbSet
            .AsNoTracking()
            .Where(item => item.UserId == userId && noteIds.Contains(item.NoteId))
            .ToListAsync(cancellationToken);

        return progress.ToDictionary(item => item.NoteId);
    }
}
