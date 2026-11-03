using TechNotes.Domain.Entities.Notes;

namespace TechNotes.Application.Interfaces.Repositories;

public interface ICategoryRepository : IGenericRepository<Category>
{
    Task<IReadOnlyList<Category>> GetByUserIdAsync(
        Guid userId,
        CancellationToken cancellationToken = default);

    Task<Category?> GetForUserAsync(
        Guid categoryId,
        Guid userId,
        CancellationToken cancellationToken = default);

    Task<bool> NameExistsAsync(
        Guid userId,
        string name,
        Guid? excludeCategoryId = null,
        CancellationToken cancellationToken = default);
}
