using Microsoft.EntityFrameworkCore;
using TechNotes.Application.Interfaces.Repositories;
using TechNotes.Persistence.Data;

namespace TechNotes.Persistence.Repositories;

public class GenericRepository<T>(AppDbContext context) : IGenericRepository<T>
    where T : class
{
    protected readonly DbSet<T> DbSet = context.Set<T>();

    // Retrieves one entity by its primary key.
    public async Task<T?> GetByIdAsync(Guid id,CancellationToken cancellationToken = default)
    {
        return await DbSet.FindAsync([id],cancellationToken);
    }

    // Retrieves all entities from the corresponding table.
    public async Task<IEnumerable<T>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await DbSet
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    // Adds a new entity to the current database context.
    public async Task AddAsync(T entity, CancellationToken cancellationToken = default)
    {
        await DbSet.AddAsync(entity, cancellationToken);
    }

    // Marks an entity as modified.
    public void Update(T entity)
    {
        DbSet.Update(entity);
    }

    // Marks an entity for deletion.
    public void Delete(T entity)
    {
        DbSet.Remove(entity);
    }

    // Saves all tracked changes through the DbContext.
    public async Task SaveChangesAsync(
        CancellationToken cancellationToken = default)
    {
        await context.SaveChangesAsync(cancellationToken);
    }
}