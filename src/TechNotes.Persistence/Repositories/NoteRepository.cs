using Microsoft.EntityFrameworkCore;
using TechNotes.Application.Interfaces.Repositories;
using TechNotes.Domain.Entities.Notes;
using TechNotes.Persistence.Data;

namespace TechNotes.Persistence.Repositories
{
    public sealed class NoteRepository : GenericRepository<Note>, INoteRepository
    {
        private readonly AppDbContext _context;

        public NoteRepository(AppDbContext context) : base(context)
        {
            _context = context;
        }

        public Task<Note?> GetWithContentAsync(Guid noteId, CancellationToken cancellationToken = default) =>
            BuildContentQuery()
                .AsNoTracking()
                .FirstOrDefaultAsync(note => note.Id == noteId, cancellationToken);

        public Task<Note?> GetTrackedWithContentAsync(Guid noteId, CancellationToken cancellationToken = default) =>
            BuildContentQuery()
                .FirstOrDefaultAsync(note => note.Id == noteId, cancellationToken);

        public async Task<IEnumerable<Note>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default) =>
            await BuildContentQuery()
                .AsNoTracking()
                .Where(note => note.UserId == userId)
                .OrderByDescending(note => note.CreatedAtUtc)
                .ToListAsync(cancellationToken);

        public async Task<IEnumerable<Note>> GetAccessibleNotesAsync(Guid userId, CancellationToken cancellationToken = default) =>
            await BuildContentQuery()
                .AsNoTracking()
                .Where(note =>
                    note.UserId == userId ||
                    (note.Visibility == Domain.Enums.NoteVisibility.Public &&
                     note.Status == Domain.Enums.NoteStatus.Published) ||
                    note.Shares.Any(share => share.UserId == userId))
                .OrderByDescending(note => note.CreatedAtUtc)
                .ToListAsync(cancellationToken);

        public async Task<IEnumerable<Note>> SearchAsync(
            Guid userId,
            string searchTerm,
            CancellationToken cancellationToken = default)
        {
            var term = searchTerm.Trim();
            if (term.Length == 0)
            {
                return [];
            }

            return await BuildContentQuery()
                .AsNoTracking()
                .Where(note => note.UserId == userId && EF.Functions.ILike(note.Title, $"%{term}%"))
                .OrderByDescending(note => note.CreatedAtUtc)
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<Note>> GetPublicNotesAsync(CancellationToken cancellationToken = default) =>
            await BuildContentQuery()
                .AsNoTracking()
                .Where(note =>
                    note.Visibility == Domain.Enums.NoteVisibility.Public &&
                    note.Status == Domain.Enums.NoteStatus.Published)
                .OrderByDescending(note => note.CreatedAtUtc)
                .ToListAsync(cancellationToken);

        public async Task<IEnumerable<Note>> GetSharedWithMeAsync(Guid userId, CancellationToken cancellationToken = default) =>
            await BuildContentQuery()
                .AsNoTracking()
                .Where(note =>
                    note.UserId != userId &&
                    note.Shares.Any(share => share.UserId == userId))
                .OrderByDescending(note => note.UpdatedAtUtc ?? note.CreatedAtUtc)
                .ToListAsync(cancellationToken);

        public async Task RemoveSectionsAsync(Note note, CancellationToken cancellationToken = default)
        {
            var sections = await _context.NoteSections
                .Where(section => section.NoteId == note.Id)
                .ToListAsync(cancellationToken);

            _context.NoteSections.RemoveRange(sections);
        }

        public async Task RemoveTagsAsync(Note note, CancellationToken cancellationToken = default)
        {
            await _context.NoteTags
                .Where(noteTag => noteTag.NoteId == note.Id)
                .ExecuteDeleteAsync(cancellationToken);
        }

        public async Task AddTagsAsync(
            IEnumerable<NoteTag> noteTags,
            CancellationToken cancellationToken = default)
        {
            await _context.NoteTags.AddRangeAsync(noteTags, cancellationToken);
        }

        private IQueryable<Note> BuildContentQuery() =>
            _context.Notes
                .Include(note => note.Sections)
                    .ThenInclude(section => section.Contents)
                .Include(note => note.Sections)
                    .ThenInclude(section => section.Children)
                        .ThenInclude(child => child.Contents)
                .Include(note => note.Shares)
                .Include(note => note.Category)
                    .ThenInclude(category => category.ParentCategory)
                .Include(note => note.NoteTags)
                    .ThenInclude(noteTag => noteTag.Tag)
                .AsSplitQuery();
    }
}
