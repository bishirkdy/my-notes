using Microsoft.EntityFrameworkCore;
using TechNotes.Application.Interfaces.Repositories;
using TechNotes.Domain.Entities.Notes;
using TechNotes.Persistence.Data;

namespace TechNotes.Persistence.Repositories
{
    public sealed class NoteRepository(AppDbContext context): GenericRepository<Note>(context), INoteRepository
    {
        // Retrieves a note together with its complete section hierarchy and content.
        public async Task<Note?> GetWithContentAsync(Guid noteId, CancellationToken cancellationToken = default)
        {
            // Loads the note and all of its related data needed by the application layer.
            return await context.Notes
                //.Include(x => x.Sections)
                //    .ThenInclude(x => x.Contents)
                //.Include(x => x.NoteTags)
                //    .ThenInclude(x => x.Tag)
                //.Include(x => x.Shares)
                .AsNoTracking()
                .FirstOrDefaultAsync(
                    x => x.Id == noteId,
                    cancellationToken);
        }

        // Retrieves all notes owned by a specific user.
        public async Task<IEnumerable<Note>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            return await context.Notes
                .AsNoTracking()
                .Where(x => x.UserId == userId)
                .OrderByDescending(x => x.CreatedAtUtc)
                .ToListAsync(cancellationToken);
        }

        // Retrieves notes the user owns or has permission to access.
        public async Task<IEnumerable<Note>> GetAccessibleNotesAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            return await context.Notes
                .AsNoTracking()
                .Where(note => note.UserId == userId || note.Visibility == Domain.Enums.NoteVisibility.Public || note.Shares.Any(share => share.UserId == userId))
                .OrderByDescending(x => x.CreatedAtUtc)
                .ToListAsync(cancellationToken);
        }

        // Searches the user's notes by title.
        public async Task<IEnumerable<Note>> SearchAsync(Guid userId, string searchTerm, CancellationToken cancellationToken = default)
        {
            return await context.Notes
                .AsNoTracking()
                .Where(x => x.UserId == userId && x.Title.Contains(searchTerm))
                .OrderByDescending(x => x.CreatedAtUtc)
                .ToListAsync(cancellationToken);
        }

        // Retrieves notes that are explicitly marked as public.
        public async Task<IEnumerable<Note>> GetPublicNotesAsync(CancellationToken cancellationToken = default)
        {
            return await context.Notes
                .AsNoTracking()
                .Where(x => x.Visibility == Domain.Enums.NoteVisibility.Public)
                .OrderByDescending(x => x.CreatedAtUtc)
                .ToListAsync(cancellationToken);
        }
    }
}
