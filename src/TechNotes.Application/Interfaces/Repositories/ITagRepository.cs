using TechNotes.Domain.Entities.Notes;

namespace TechNotes.Application.Interfaces.Repositories;

public interface ITagRepository : IGenericRepository<Tag>
{
    Task<IReadOnlyList<Tag>> GetByNamesAsync(
        IReadOnlyCollection<string> names,
        CancellationToken cancellationToken = default);
}
