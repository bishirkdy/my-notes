using MediatR;
using TechNotes.Application.DTOs.Categories;
using TechNotes.Application.Features.Categories.Common;
using TechNotes.Application.Interfaces.Repositories;

namespace TechNotes.Application.Features.Categories.Queries.GetCategoryTree;

public sealed class GetCategoryTreeQueryHandler(ICategoryRepository categoryRepository)
    : IRequestHandler<GetCategoryTreeQuery, IReadOnlyList<CategoryTreeResponse>>
{
    public async Task<IReadOnlyList<CategoryTreeResponse>> Handle(
        GetCategoryTreeQuery query,
        CancellationToken cancellationToken)
    {
        var categories = await categoryRepository.GetByUserIdAsync(
            query.UserId,
            cancellationToken);

        return CategoryResponseMapper.BuildTree(categories);
    }
}
