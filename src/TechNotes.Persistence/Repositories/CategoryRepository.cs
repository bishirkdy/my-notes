using Microsoft.EntityFrameworkCore;
using TechNotes.Application.Interfaces.Repositories;
using TechNotes.Domain.Entities.Notes;
using TechNotes.Persistence.Data;

namespace TechNotes.Persistence.Repositories;

public sealed class CategoryRepository : GenericRepository<Category>, ICategoryRepository
{
    private readonly AppDbContext _context;

    public CategoryRepository(AppDbContext context) : base(context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<Category>> GetByUserIdAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        return await _context.Categories
            .AsNoTracking()
            .Include(category => category.ParentCategory)
            .Include(category => category.Children)
            .Include(category => category.Notes)
            .Where(category => category.UserId == userId)
            .OrderBy(category => category.SortOrder)
            .ThenBy(category => category.Name)
            .ToListAsync(cancellationToken);
    }

    public Task<Category?> GetForUserAsync(
        Guid categoryId,
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        return _context.Categories
            .Include(category => category.ParentCategory)
            .Include(category => category.Children)
            .Include(category => category.Notes)
            .FirstOrDefaultAsync(
                category => category.Id == categoryId && category.UserId == userId,
                cancellationToken);
    }

    public Task<bool> NameExistsAsync(
        Guid userId,
        string name,
        Guid? excludeCategoryId = null,
        CancellationToken cancellationToken = default)
    {
        return _context.Categories
            .AnyAsync(
                category =>
                    category.UserId == userId &&
                    (!excludeCategoryId.HasValue || category.Id != excludeCategoryId.Value) &&
                    EF.Functions.ILike(category.Name, name),
                cancellationToken);
    }
}
