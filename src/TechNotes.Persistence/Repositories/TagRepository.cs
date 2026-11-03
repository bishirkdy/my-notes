using Microsoft.EntityFrameworkCore;
using TechNotes.Application.Interfaces.Repositories;
using TechNotes.Domain.Entities.Notes;
using TechNotes.Persistence.Data;

namespace TechNotes.Persistence.Repositories;

public sealed class TagRepository : GenericRepository<Tag>, ITagRepository
{
    private readonly AppDbContext _context;

    public TagRepository(AppDbContext context) : base(context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<Tag>> GetByNamesAsync(
        IReadOnlyCollection<string> names,
        CancellationToken cancellationToken = default)
    {
        if (names.Count == 0)
        {
            return [];
        }

        var requested = new HashSet<string>(names, StringComparer.OrdinalIgnoreCase);
        var tags = await _context.Tags.ToListAsync(cancellationToken);

        return tags
            .Where(tag => requested.Contains(tag.Name))
            .ToList();
    }
}
